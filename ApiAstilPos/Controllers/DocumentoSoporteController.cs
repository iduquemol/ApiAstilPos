using ApiAstilPos.Models;
using ApiAstilPos.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using QRCoder;
using System.Data;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using JsonException = Newtonsoft.Json.JsonException;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class DocumentoSoporteController : ControllerBase
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly IConfiguration _configuration;
        private readonly ILogger<DocumentoSoporteController> _logger;

        public DocumentoSoporteController(IConfiguration configuration, ILogger<DocumentoSoporteController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("documentos-soporte")]
        public async Task<IActionResult> GetDocumentosSoporte([FromQuery] long? idDsa = null)
        {
            _logger.LogInformation("Obteniendo lista de documentos soporte");

            try
            {
                var documentosSoporte = new List<DocumentoSoporte>();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_documentosSoporteId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idDsa", (object?)idDsa ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var json = reader.IsDBNull(reader.GetOrdinal("documentosSoporte"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("documentosSoporte"));

                                documentosSoporte = JsonConvert.DeserializeObject<List<DocumentoSoporte>>(json) ?? new List<DocumentoSoporte>();
                            }
                        }
                    }
                }

                _logger.LogInformation($"Documentos soporte obtenidos: {documentosSoporte.Count}");
                return Ok(documentosSoporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener documentos soporte");
                return StatusCode(500, new { error = true, idDsa = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("obtener-documento-soporte")]
        public async Task<IActionResult> GetByIdDocumentoSoporteId([FromBody] JsonElement request)
        {
            _logger.LogInformation("Procesando solicitud para obtener documento soporte por ID");

            try
            {
                long idDsa = 0;
                if (request.TryGetProperty("iddsa", out var dsaElement))
                {
                    idDsa = dsaElement.GetInt64();
                }
                else if (request.TryGetProperty("idDsa", out var dsaElementPascal))
                {
                    idDsa = dsaElementPascal.GetInt64();
                }

                if (idDsa == 0)
                {
                    return BadRequest("El parametro iddsa es requerido o invalido.");
                }

                DocumentoSoporte documentoSoporte = null;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_documentosSoporteId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idDsa", idDsa);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonDocumentoSoporte = reader.IsDBNull(reader.GetOrdinal("documentosSoporte"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("documentosSoporte"));

                                if (!string.IsNullOrEmpty(jsonDocumentoSoporte))
                                {
                                    if (jsonDocumentoSoporte.TrimStart().StartsWith("["))
                                    {
                                        var listaDocumentos = JsonConvert.DeserializeObject<List<DocumentoSoporte>>(jsonDocumentoSoporte);
                                        documentoSoporte = listaDocumentos?.FirstOrDefault();
                                    }
                                    else
                                    {
                                        documentoSoporte = JsonConvert.DeserializeObject<DocumentoSoporte>(jsonDocumentoSoporte);
                                    }
                                }
                            }
                        }
                    }
                }

                _logger.LogInformation("Documento soporte obtenido correctamente.");
                return Ok(documentoSoporte);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener documento soporte: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("dsa-create")]
        public async Task<IActionResult> CreateDsa([FromBody] DocumentoSoporte dsa)
        {
            _logger.LogInformation("Creando un nuevo Documento Soporte de Adquisición");

            try
            {
                string requestBody = JsonConvert.SerializeObject(dsa, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                object dsaId = null;
                object bodyDian = null;
                byte[] attachedDocumentBytes = null;
                string cufe = string.Empty;
                string firmaDigital = string.Empty;
                string qrCode = string.Empty;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_Create_Dsa", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@dsa", requestBody ?? (object)DBNull.Value);

                        dsaId = await command.ExecuteScalarAsync();
                        _logger.LogInformation("Documento Soporte creado correctamente.");
                    }

                    if (dsaId == null || Convert.ToInt64(dsaId) <= 0)
                    {
                        _logger.LogError("No se pudo obtener un ID de DSA válido.");
                        return BadRequest("Error: No se pudo crear el Documento Soporte.");
                    }

                    using (var command2 = new SqlCommand("sp_Read_DsaIdDian", connection))
                    {
                        command2.CommandType = CommandType.StoredProcedure;
                        command2.Parameters.AddWithValue("@idDsa", dsaId);
                        command2.Parameters.AddWithValue("@idMetodoDian", 5); 

                        using (var reader2 = await command2.ExecuteReaderAsync())
                        {
                            if (await reader2.ReadAsync())
                            {
                                bodyDian = reader2.IsDBNull(0) ? null : reader2.GetString(0);
                            }
                        }
                        _logger.LogInformation($"Body DIAN DSA obtenido: {bodyDian}");
                    }

                    if (bodyDian == null || string.IsNullOrEmpty(bodyDian.ToString()))
                    {
                        _logger.LogError($"No se pudo obtener el Body DIAN para el ID DSA {dsaId}");
                        return BadRequest("Error: No se pudo generar el cuerpo para la DIAN.");
                    }

                    _logger.LogInformation($"JSON enviado a la API externa para el Documento Soporte {dsaId}:\n{bodyDian}");

                    var apiResponse = await CallExternalApiAsync(bodyDian.ToString());

                    if (apiResponse.IsSuccess)
                    {
                        _logger.LogInformation("API externa llamada exitosamente.");

                        string attachedDocumentBase64 = ExtraerAttachedDocumentBase64(apiResponse.contentResponse);

                        if (!string.IsNullOrEmpty(attachedDocumentBase64))
                        {
                            attachedDocumentBytes = Convert.FromBase64String(attachedDocumentBase64);
                            _logger.LogInformation($"Documento adjunto obtenido, tamaño: {attachedDocumentBytes.Length} bytes");
                        }

                        using (var command = new SqlCommand("sp_Insert_responseDian", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@responseDianJson", apiResponse.contentResponse ?? (object)DBNull.Value);

                            var pIdResponseDian = command.Parameters.Add("@idResponseDian", SqlDbType.BigInt);
                            pIdResponseDian.Direction = ParameterDirection.InputOutput;
                            pIdResponseDian.Value = 0;

                            var pCufe = command.Parameters.Add("@cufe", SqlDbType.NVarChar, 255);
                            pCufe.Direction = ParameterDirection.Output;

                            var pFirmaDigital = command.Parameters.Add("@firmaDigital", SqlDbType.NVarChar, -1);
                            pFirmaDigital.Direction = ParameterDirection.Output;

                            var pQrCode = command.Parameters.Add("@qrCode", SqlDbType.NVarChar, -1);
                            pQrCode.Direction = ParameterDirection.Output;

                            await command.ExecuteNonQueryAsync();

                            cufe = pCufe.Value != DBNull.Value ? pCufe.Value.ToString() : string.Empty;
                            qrCode = pQrCode.Value != DBNull.Value ? pQrCode.Value.ToString() : string.Empty;

                            _logger.LogInformation("Response Factura Dian para DSA creada correctamente.");
                        }

                        return Ok(new
                        {
                            message = "Documento Soporte creado y procesado correctamente en la DIAN",
                            idDsa = dsaId,
                            numeroDocumentoDian = apiResponse.numeroFacturaDian
                        });
                    }
                    else
                    {
                        _logger.LogWarning($"Error en API externa: {apiResponse.ErrorMessage}");
                        return Ok(new
                        {
                            message = "Documento Soporte creado, pero hubo un error en la API externa",
                            idDsa = dsaId,
                            externalApiError = apiResponse.ErrorMessage
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear Documento Soporte: {ex.Message}");
                return BadRequest(new { error = true, mensaje = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        private async Task<ApiResponse> CallExternalApiAsync(string originalRequestBody)
        {
            try
            {
                ResponseDian responseDian = new ResponseDian();
                string apiUrl = _configuration["ApiExterna:DsaUrl"];
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
                    _logger.LogWarning("Contenido XML vacío para extraer attached_document_base64_bytes");
                    return string.Empty;
                }

                var jsonResponse = JObject.Parse(xmlContent);

                var attachedDocumentBase64 = jsonResponse["attached_document_base64_bytes"]?.ToString();

                if (!string.IsNullOrEmpty(attachedDocumentBase64))
                {
                    _logger.LogInformation("attached_document_base64_bytes extraído exitosamente del JSON");
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
            catch (JsonException jsonEx)
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