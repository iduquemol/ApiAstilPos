using ApiAstilPos.Models;
using ApiAstilPos.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.Text.Json;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class EnviarEventoController : ControllerBase
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly IConfiguration _configuration;
        private readonly ILogger<EnviarEventoController> _logger;

        public EnviarEventoController(IConfiguration configuration, ILogger<EnviarEventoController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpPost("enviar-evento")]
        public async Task<IActionResult> EnviarEventoDian([FromBody] JsonElement request)
        {
            _logger.LogInformation("Procesando solicitud para enviar evento DIAN");

            try
            {
                long idTipoDocumentoExterno = 7;
                if (request.TryGetProperty("idTipoDocumentoExterno", out var p1) || request.TryGetProperty("IdTipoDocumentoExterno", out p1))
                    idTipoDocumentoExterno = p1.GetInt64();

                long idVenta = 0;
                if (request.TryGetProperty("idventa", out var p2) || request.TryGetProperty("IdVenta", out p2) || request.TryGetProperty("idVenta", out p2))
                    idVenta = p2.GetInt64();

                long idUsuario = 1;
                if (request.TryGetProperty("idUsuario", out var p3) || request.TryGetProperty("IdUsuario", out p3) || request.TryGetProperty("idusuario", out p3))
                    idUsuario = p3.GetInt64();

                long idEventoDian = 0;
                if (request.TryGetProperty("idEventoDian", out var p4) || request.TryGetProperty("IdEventoDian", out p4) || request.TryGetProperty("ideventodian", out p4))
                    idEventoDian = p4.GetInt64();

                long? idConceptoRechazo = null;
                if ((request.TryGetProperty("idConceptoRechazo", out var p5) || request.TryGetProperty("IdConceptoRechazo", out p5)) && p5.ValueKind != JsonValueKind.Null)
                    idConceptoRechazo = p5.GetInt64();

                if (idVenta == 0 || idEventoDian == 0)
                {
                    return BadRequest("Los parámetros idventa e idEventoDian son requeridos y válidos.");
                }

                string codigoEventoDian = string.Empty;
                string eventoDianJson = string.Empty;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_Read_EventoIdDian", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idTipoDocumentoExterno", idTipoDocumentoExterno);
                        command.Parameters.AddWithValue("@idVenta", idVenta);
                        command.Parameters.AddWithValue("@idUsuario", idUsuario);
                        command.Parameters.AddWithValue("@idEventoDian", idEventoDian);
                        command.Parameters.AddWithValue("@idConceptoRechazo", idConceptoRechazo.HasValue ? (object)idConceptoRechazo.Value : DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                codigoEventoDian = reader.IsDBNull(reader.GetOrdinal("codigoEventoDian"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("codigoEventoDian"));

                                
                                eventoDianJson = reader.IsDBNull(reader.GetOrdinal("eventoDian"))
                                    ? string.Empty
                                    : reader.GetString(reader.GetOrdinal("eventoDian"));
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(eventoDianJson) || string.IsNullOrEmpty(codigoEventoDian))
                    {
                        _logger.LogError($"No se pudo generar el JSON del evento DIAN para la venta {idVenta}");
                        return BadRequest("Error: No se pudo generar el cuerpo del evento para la DIAN desde la base de datos.");
                    }

                    _logger.LogInformation($"JSON del Evento DIAN generado [{codigoEventoDian}]:\n{eventoDianJson}");

                    // Seleccionar la URL del API externa de Soenac según el código del evento 
                    string apiUrl = GetSoenacEventUrl(codigoEventoDian.Trim());

                    if (string.IsNullOrEmpty(apiUrl))
                    {
                        return BadRequest($"No hay una URL configurada en appsettings para el código de evento DIAN: {codigoEventoDian}");
                    }

                    //  Consumo del API externa
                    var apiResponse = await CallExternalApiCustomUrlAsync(apiUrl, eventoDianJson);

                    if (apiResponse.IsSuccess)
                    {
                        _logger.LogInformation("API externa de eventos llamada exitosamente. Guardando respuesta con sp_Insert_responseDian...");

                        long idResponseDian = 0;
                        string cufeResult = string.Empty;
                        string firmaDigitalResult = string.Empty;
                        string qrCodeResult = string.Empty;

                        using (var commandResponse = new SqlCommand("sp_Insert_responseDian", connection))
                        {
                            commandResponse.CommandType = CommandType.StoredProcedure;
                            commandResponse.Parameters.AddWithValue("@responseDianJson", apiResponse.contentResponse ?? (object)DBNull.Value);

                            var pIdResponseDian = new SqlParameter("@idResponseDian", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                            commandResponse.Parameters.Add(pIdResponseDian);

                            var pCufe = new SqlParameter("@cufe", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };
                            commandResponse.Parameters.Add(pCufe);

                            var pFirmaDigital = new SqlParameter("@firmaDigital", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };
                            commandResponse.Parameters.Add(pFirmaDigital);

                            var pQrCode = new SqlParameter("@qrCode", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };
                            commandResponse.Parameters.Add(pQrCode);

                            await commandResponse.ExecuteNonQueryAsync();

                            idResponseDian = pIdResponseDian.Value != DBNull.Value ? Convert.ToInt64(pIdResponseDian.Value) : 0;
                            cufeResult = pCufe.Value != DBNull.Value ? pCufe.Value.ToString() : string.Empty;
                            firmaDigitalResult = pFirmaDigital.Value != DBNull.Value ? pFirmaDigital.Value.ToString() : string.Empty;
                            qrCodeResult = pQrCode.Value != DBNull.Value ? pQrCode.Value.ToString() : string.Empty;

                            _logger.LogInformation($"Response Evento Dian registrado correctamente. ID: {idResponseDian}, CUFE: {cufeResult}");
                        }

                        return Ok(new
                        {
                            message = "Evento DIAN enviado y respuesta registrada correctamente",
                            codigoEventoDian = codigoEventoDian,
                            idResponseDian = idResponseDian,
                            externalApiResponse = apiResponse.contentResponse
                        });
                    }
                    else
                    {
                        _logger.LogWarning($"Error en API externa de eventos: {apiResponse.ErrorMessage}");
                        return Ok(new
                        {
                            message = "El evento se intentó enviar, pero hubo un error en la API externa de Soenac",
                            codigoEventoDian = codigoEventoDian,
                            externalApiError = apiResponse.ErrorMessage
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al procesar el envío del evento DIAN: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        private string GetSoenacEventUrl(string codigoEvento) => codigoEvento switch
        {
            "030" => _configuration["ApiExterna:Event030Url"],
            "031" => _configuration["ApiExterna:Event031Url"],
            "032" => _configuration["ApiExterna:Event032Url"],
            "033" => _configuration["ApiExterna:Event033Url"],
            "034" => _configuration["ApiExterna:Event034Url"],
            _ => string.Empty
        };

        private async Task<ApiResponse> CallExternalApiCustomUrlAsync(string apiUrl, string jsonBody)
        {
            try
            {
                var bearerToken = _configuration["ApiExterna:BearerToken"];

                if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(bearerToken))
                {
                    return new ApiResponse { IsSuccess = false, ErrorMessage = "URL de evento o Bearer Token no configurados en appsettings" };
                }

                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

                _logger.LogInformation($"Llamando a API externa de eventos: {apiUrl}");

                var response = await httpClient.PostAsync(apiUrl, content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse
                    {
                        IsSuccess = true,
                        contentResponse = responseContent
                    };
                }
                else
                {
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"API externa falló: {response.StatusCode} - {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse { IsSuccess = false, ErrorMessage = $"Excepción de conexión: {ex.Message}" };
            }
        }
    }
}