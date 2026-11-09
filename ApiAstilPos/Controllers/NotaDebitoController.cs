using ApiAstilPos.Models;
using ApiAstilPos.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System.Data;
using System.Text;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class NotaDebitoController : ControllerBase
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotaDebitoController> _logger;

        public NotaDebitoController(EmailService emailService, IConfiguration configuration, ILogger<NotaDebitoController> logger)
        {
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("notasdebito")]
        public async Task<IActionResult> GetNotaDebito([FromQuery] long? idNotaDebito = null)
        {
            _logger.LogInformation("Obteniendo lista de notas débito");

            try
            {
                var notasDebito = new List<NotaDebito>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_notasDebitoId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idNotaDebito", (object?)idNotaDebito ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var json = reader.IsDBNull(reader.GetOrdinal("notasDebito"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("notasDebito"));

                                notasDebito = JsonConvert.DeserializeObject<List<NotaDebito>>(json) ?? new List<NotaDebito>();
                            }
                        }
                    }
                }

                _logger.LogInformation($"Notas débito obtenidas: {notasDebito.Count}");
                return Ok(notasDebito);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener notas débito");
                return StatusCode(500, new { error = true, idNotaDebito = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpGet("tipos-documento-notas-debito")]
        public async Task<IActionResult> GetTiposDocumentoNotaDebito([FromQuery] long? idTipoDocumentoExterno = null)
        {
            _logger.LogInformation("Obteniendo lista de tipos de documento notas débito");

            try
            {
                string jsonResult = "[]";
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposDocumentoNotasDebito", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idTipoDocumentoExterno", (object?)idTipoDocumentoExterno ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                jsonResult = reader.IsDBNull(reader.GetOrdinal("tiposDocumentoVentas"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("tiposDocumentoVentas"));
                            }
                        }
                    }
                }

                _logger.LogInformation($"Tipos de documento notas débito obtenidos correctamente");
                return Content(jsonResult, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de documentos notas débito");
                return StatusCode(500, new { error = true, idTipoDocumentoExterno = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("obtener-nota-debito")]
        public async Task<IActionResult> PostNotaDebitoId([FromBody] JObject request)
        {
            _logger.LogInformation("Procesando solicitud para obtener nota débito por ID");

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

                        _logger.LogInformation("Nota débito obtenida correctamente.");
                        return Ok(venta);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener nota débito: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("print-nota-debito")]
        public async Task<IActionResult> PrintNotaDebitoId([FromBody] JObject request)
        {
            _logger.LogInformation("Procesando solicitud para imprimir nota débito por ID");

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

                        _logger.LogInformation("Nota débito para imprimir obtenida correctamente.");
                        return Ok(printVenta);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener nota débito: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("notadebito")]
        public async Task<IActionResult> CreateNotaDebito([FromBody] NotaDebito notaDebito)
        {
            _logger.LogInformation("Creando una nueva nota débito");

            if (notaDebito == null)
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

                string requestBody = JsonConvert.SerializeObject(notaDebito, jsonSettings);
                _logger.LogInformation($"Cuerpo de la solicitud enviado al SP: {requestBody}");

                object idNotaDebitoObj = null;
                object bodyDian = null;
                byte[] attachedDocumentBytes = null;
                string cufe = string.Empty;
                string qrCode = string.Empty;
                long idMetodoDian = 6; 
                long idNotaDebito = 0;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_notaDebito", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        var parameter = command.Parameters.Add("@notaDebito", SqlDbType.NVarChar, -1);
                        parameter.Value = string.IsNullOrEmpty(requestBody) ? DBNull.Value : requestBody;

                        idNotaDebitoObj = await command.ExecuteScalarAsync();
                    }

                    if (idNotaDebitoObj == null || !long.TryParse(idNotaDebitoObj.ToString(), out idNotaDebito) || idNotaDebito <= 0)
                    {
                        _logger.LogWarning("El Stored Procedure no generó un idNotaDebito válido.");
                        return BadRequest("No se pudo crear la Nota Débito. Verifique que la Venta relacionada sea válida.");
                    }

                    _logger.LogInformation($"Nota Débito creada correctamente con ID: {idNotaDebito}");

                    // Lectura del JSON para la DIAN
                    using (var command2 = new SqlCommand("sp_Read_notasDebitoIdDian", connection))
                    {
                        command2.CommandType = CommandType.StoredProcedure;
                        command2.Parameters.AddWithValue("@idNotaDebito", idNotaDebito);
                        command2.Parameters.AddWithValue("@idMetodoDian", idMetodoDian);

                        using (var reader2 = await command2.ExecuteReaderAsync())
                        {
                            if (await reader2.ReadAsync())
                            {
                                bodyDian = reader2.IsDBNull(0) ? null : reader2.GetString(0);
                            }
                        }
                    }

                    if (bodyDian == null || string.IsNullOrEmpty(bodyDian.ToString()))
                    {
                        _logger.LogError($"No se pudo obtener el Body DIAN para el ID Nota Débito {idNotaDebito}");
                        return BadRequest("Error: No se pudo generar el cuerpo para la DIAN.");
                    }

                    _logger.LogInformation($"JSON enviado a la API externa para Nota Débito {idNotaDebito}:\n{bodyDian}");

                    // Consumo API externa
                    var apiResponse = await CallExternalApiAsync(bodyDian.ToString(), idNotaDebito, idMetodoDian);

                    if (apiResponse.IsSuccess)
                    {
                        _logger.LogInformation("API externa llamada exitosamente.");

                        string attachedDocumentBase64 = ExtraerAttachedDocumentBase64(apiResponse.contentResponse);

                        if (!string.IsNullOrEmpty(attachedDocumentBase64))
                        {
                            attachedDocumentBytes = Convert.FromBase64String(attachedDocumentBase64);
                            _logger.LogInformation($"Documento adjunto obtenido, tamaño: {attachedDocumentBytes.Length} bytes");
                        }

                        // Guardar respuesta DIAN
                        using (var commandInsert = new SqlCommand("sp_Insert_responseDian", connection))
                        {
                            commandInsert.CommandType = CommandType.StoredProcedure;
                            commandInsert.Parameters.AddWithValue("@responseDianJson", apiResponse.contentResponse ?? (object)DBNull.Value);

                            var pIdResponseDian = commandInsert.Parameters.Add("@idResponseDian", SqlDbType.BigInt);
                            pIdResponseDian.Direction = ParameterDirection.InputOutput;
                            pIdResponseDian.Value = 0;

                            var pCufe = commandInsert.Parameters.Add("@cufe", SqlDbType.NVarChar, 255);
                            pCufe.Direction = ParameterDirection.Output;

                            var pFirmaDigital = commandInsert.Parameters.Add("@firmaDigital", SqlDbType.NVarChar, -1);
                            pFirmaDigital.Direction = ParameterDirection.Output;

                            var pQrCode = commandInsert.Parameters.Add("@qrCode", SqlDbType.NVarChar, -1);
                            pQrCode.Direction = ParameterDirection.Output;

                            await commandInsert.ExecuteNonQueryAsync();

                            cufe = pCufe.Value != DBNull.Value ? pCufe.Value.ToString() : string.Empty;
                            qrCode = pQrCode.Value != DBNull.Value ? pQrCode.Value.ToString() : string.Empty;

                            _logger.LogInformation("Response Factura Dian creada correctamente para Nota Débito.");
                        }

                        // Obtener datos para el PDF y Correo (puedes usar el SP sp_Print_ventaId o crear sp_Print_notaDebitoId si aplica)
                        PrintVenta printVenta = null;
                        using (var commandPrint = new SqlCommand("sp_Print_ventaId", connection))
                        {
                            commandPrint.CommandType = CommandType.StoredProcedure;
                            commandPrint.Parameters.AddWithValue("@idVenta", idNotaDebito);
                            commandPrint.Parameters.AddWithValue("@idMetodoDian", idMetodoDian);

                            using (var readerPrint = await commandPrint.ExecuteReaderAsync())
                            {
                                while (await readerPrint.ReadAsync())
                                {
                                    var jsonVenta = readerPrint.IsDBNull(readerPrint.GetOrdinal("venta"))
                                        ? "[]"
                                        : readerPrint.GetString(readerPrint.GetOrdinal("venta"));
                                    printVenta = JsonConvert.DeserializeObject<PrintVenta>(jsonVenta);
                                }
                            }
                        }

                        // Generación de PDF y envío de Email
                        if (printVenta != null)
                        {
                            var pdfService = new FacturaPdfService();
                            printVenta.Cufe = cufe;
                            printVenta.CodigoQR = qrCode;
                            byte[] pdfBytes = pdfService.GenerarPdfFactura(printVenta, Convert.ToInt32(idMetodoDian));

                            _logger.LogInformation("PDF de Nota Débito generado correctamente.");

                            try
                            {
                                var facturaEmailDto = new FacturaEmailDto
                                {
                                    Email = printVenta.ClienteEmail,
                                    NombreCliente = printVenta.ClienteRazonSocial,
                                    NumeroDocumento = apiResponse.numeroFacturaDian,
                                    SubjectEmail = printVenta.SubjectEmail ?? string.Empty,
                                    Total = printVenta.TotalVenta,
                                    PdfAttachment = pdfBytes,
                                    PdfFileName = $"NotaDebito_{apiResponse.numeroFacturaDian}.pdf",
                                    XmlAttachment = attachedDocumentBytes,
                                    XmlFileName = $"NotaDebito_{apiResponse.numeroFacturaDian}.xml",
                                    FacturadorNombre = printVenta.FacturadorNombre ?? string.Empty,
                                };

                                await _emailService.SendFacturaEmailAsync(facturaEmailDto, Convert.ToInt32(idMetodoDian));
                                _logger.LogInformation($"Email enviado exitosamente a {facturaEmailDto.Email}");
                            }
                            catch (Exception emailEx)
                            {
                                _logger.LogWarning($"Nota Débito creada pero falló el envío de email: {emailEx.Message}");
                            }
                        }

                        return Ok(new
                        {
                            message = "Nota Débito creada correctamente",
                            idNotaDebito,
                            numeroDocumentoDian = apiResponse.numeroFacturaDian
                        });
                    }
                    else
                    {
                        _logger.LogWarning($"Error en API externa: {apiResponse.ErrorMessage}");
                        return Ok(new
                        {
                            message = "Nota Débito creada correctamente, pero hubo un error en la API externa",
                            idNotaDebito,
                            externalApiError = apiResponse.ErrorMessage
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al crear nota débito");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        private async Task<ApiResponse> CallExternalApiAsync(string originalRequestBody, object documentoId, long IdMetodoDian)
        {
            try
            {
                ResponseDian responseDian = new ResponseDian();
                string apiUrl = string.Empty;

                if (IdMetodoDian == 1)
                {
                    apiUrl = _configuration["ApiExterna:InvoiceUrl"];
                }
                else if (IdMetodoDian == 2)
                {
                    apiUrl = _configuration["ApiExterna:PosUrl"];
                }
                else if (IdMetodoDian == 3)
                {
                    apiUrl = _configuration["ApiExterna:NotaCUrl"];
                }
                else if (IdMetodoDian == 6)
                {
                    apiUrl = _configuration["ApiExterna:NotaDUrl"];
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
                        ErrorMessage = $"API externa falló: {response.StatusCode} - {errorContent} - {apiUrl}"
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

        private string ExtraerAttachedDocumentBase64(string xmlContent)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlContent))
                {
                    return string.Empty;
                }

                var jsonResponse = JObject.Parse(xmlContent);

                var attachedDocumentBase64 = jsonResponse["attached_document_base64_bytes"]?.ToString() ??
                                           jsonResponse["attached_document"]?.ToString();

                if (!string.IsNullOrEmpty(attachedDocumentBase64))
                {
                    return attachedDocumentBase64;
                }

                var xmlString = jsonResponse["xml"]?.ToString() ??
                               jsonResponse["AttachedDocument"]?.ToString() ??
                               xmlContent;

                if (xmlString.Contains("attached_document_base64_bytes"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        xmlString,
                        @"<attached_document_base64_bytes>(.*?)</attached_document_base64_bytes>",
                        System.Text.RegularExpressions.RegexOptions.Singleline
                    );

                    if (match.Success && match.Groups.Count > 1)
                    {
                        attachedDocumentBase64 = match.Groups[1].Value.Trim();
                        _logger.LogInformation($"attached_document_base64_bytes extraído del XML, tamaño: {attachedDocumentBase64.Length} caracteres");
                        return attachedDocumentBase64;
                    }
                }

                _logger.LogWarning("No se encontró attached_document_base64_bytes en el contenido");
                return string.Empty;
            }
            catch (Newtonsoft.Json.JsonException jsonEx)
            {
                _logger.LogError($"Error al parsear JSON para extraer attached_document_base64_bytes: {jsonEx.Message}");

                try
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        xmlContent,
                        @"<attached_document_base64_bytes>(.*?)</attached_document_base64_bytes>",
                        System.Text.RegularExpressions.RegexOptions.Singleline
                    );

                    if (match.Success && match.Groups.Count > 1)
                    {
                        var result = match.Groups[1].Value.Trim();
                        _logger.LogInformation($"attached_document_base64_bytes extraído del XML puro, tamaño: {result.Length} caracteres");
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al procesar XML: {ex.Message}");
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado al extraer attached_document_base64_bytes: {ex.Message}");
                return string.Empty;
            }
        }
    }
}