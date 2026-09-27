using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class NotaCreditoController : ControllerBase
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotaCreditoController> _logger;

        public NotaCreditoController(IConfiguration configuration, ILogger<NotaCreditoController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpPost("obtener-nota-credito")]
        public async Task<IActionResult> PostNotaCreditoId([FromBody] JObject request)
        {
            _logger.LogInformation("Procesando solicitud para obtener venta por ID");

            try
            {
                var idVenta = request["idventa"]?.Value<long>() ?? 0;
                _logger.LogInformation($"ID Venta: {idVenta}");

                var venta = new Venta();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_ventaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idVenta", idVenta);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonVenta = reader.IsDBNull(reader.GetOrdinal("venta"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("venta"));
                                venta = JsonConvert.DeserializeObject<Venta>(jsonVenta);
                            }
                        }

                        _logger.LogInformation("Venta obtenida correctamente.");
                        return Ok(venta);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener venta: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("print-nota-credito")]
        public async Task<IActionResult> PrintNotaCreditoId([FromBody] JObject request)
        {
            _logger.LogInformation("Procesando solicitud para imprimir venta por ID");

            try
            {
                var idVenta = request["idventa"]?.Value<long>() ?? 0;
                _logger.LogInformation($"ID Venta: {idVenta}");

                var printVenta = new PrintVenta();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Print_ventaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idVenta", idVenta);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonVenta = reader.IsDBNull(reader.GetOrdinal("venta"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("venta"));
                                printVenta = JsonConvert.DeserializeObject<PrintVenta>(jsonVenta);
                            }
                        }

                        _logger.LogInformation("Venta para imprimir obtenida correctamente.");
                        return Ok(printVenta);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener venta: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("notacredito")]
        public async Task<IActionResult> CreateNotaCredito([FromBody] NotaCredito notaCredito)
        {
            _logger.LogInformation("Creando una nueva nota credito");

            if (notaCredito == null)
            {
                return BadRequest("El cuerpo de la solicitud no puede ser nulo.");
            }

            try
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    DateFormatString = "yyyy-MM-dd",
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                string requestBody = JsonConvert.SerializeObject(notaCredito, jsonSettings);
                _logger.LogInformation($"Cuerpo de la solicitud enviado al SP: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_notaCredito", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        var parameter = command.Parameters.Add("@notaCredito", SqlDbType.NVarChar, -1);
                        parameter.Value = string.IsNullOrEmpty(requestBody) ? DBNull.Value : requestBody;

                        object result = await command.ExecuteScalarAsync();

                        if (result != null && long.TryParse(result.ToString(), out long idNotaCredito) && idNotaCredito > 0)
                        {
                            _logger.LogInformation($"Nota Credito creada correctamente con ID: {idNotaCredito}");
                            return Ok(new { message = "Nota Credito creada correctamente", idNotaCredito });
                        }
                        else
                        {
                            _logger.LogWarning("El Stored Procedure no generó un idNotaCredito válido. (Posible falta de idVenta o falla en auditoría).");
                            return BadRequest("No se pudo crear la Nota Crédito. Verifique que la Venta relacionada sea válida.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al crear nota credito");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        private async Task<ApiResponse> CallExternalApiAsync(string originalRequestBody, object facturaId, long IdMetodoDian)
        {
            try
            {
                ResponseDian responseDian = new ResponseDian();
                string apiUrl = String.Empty;
                if (IdMetodoDian == 1)
                {
                    apiUrl = _configuration["ApiExterna:InvoiceUrl"];
                }
                else if (IdMetodoDian == 2)
                {
                    apiUrl = _configuration["ApiExterna:PosUrl"];
                }

                var bearerToken = _configuration["ApiExterna:BearerToken"];

                if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(bearerToken))
                {
                    _logger.LogError("URL de API externa o Bearer Token no configurados");
                    return new ApiResponse { IsSuccess = false, ErrorMessage = "Configuración de API externa faltante" };
                }

                var content = new StringContent(originalRequestBody, Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

                _logger.LogInformation($"Llamando a API externa: {apiUrl}");

                var response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"API externa respondió exitosamente: {response.StatusCode}");
                    responseDian = JsonConvert.DeserializeObject<ResponseDian>(responseContent);

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        numeroFacturaDian = responseDian.Number,
                        contentResponse = responseContent,
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error en API externa: {response.StatusCode} - {errorContent}");

                    return new ApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"API externa falló: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"Error de conexión con API externa: {httpEx.Message}");
                return new ApiResponse { IsSuccess = false, ErrorMessage = $"Error de conexión: {httpEx.Message}" };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado al llamar API externa: {ex.Message}");
                return new ApiResponse { IsSuccess = false, ErrorMessage = $"Error inesperado: {ex.Message}" };
            }
        }
    }
}