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

                    return Ok(new
                    {
                        message = "Documento Soporte creado correctamente.",
                        idDsa = dsaId
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear Documento Soporte: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /*
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

                    using (var command2 = new SqlCommand("sp_Read_dsaldDian", connection))
                    {
                        command2.CommandType = CommandType.StoredProcedure;
                        command2.Parameters.AddWithValue("@idDsa", dsaId);
                        command2.Parameters.AddWithValue("@idMetodoDian", dsa.IdMetodoDian);

                        bodyDian = await command2.ExecuteScalarAsync();
                        _logger.LogInformation($"Body DIAN DSA: {bodyDian}");
                    }

                    if (bodyDian == null)
                    {
                        _logger.LogError($"No se pudo obtener el Body DIAN para el ID DSA {dsaId}");
                        return BadRequest("Error: No se pudo generar el cuerpo para la DIAN.");
                    }

                    _logger.LogInformation($"JSON enviado a la API externa para el Documento Soporte {dsaId}:\n{bodyDian}");

                    var apiResponse = await CallExternalApiAsync(bodyDian.ToString(), dsaId, dsa.IdMetodoDian);

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

                            var idResponseDianObtenido = pIdResponseDian.Value;
                            cufe = pCufe.Value != DBNull.Value ? pCufe.Value.ToString() : string.Empty;
                            qrCode = pQrCode.Value != DBNull.Value ? pQrCode.Value.ToString() : string.Empty;

                            _logger.LogInformation("Response Factura Dian para DSA creada correctamente.");
                        }

                        /* 
                        PrintDsa printDsa = null;
                        using (var command = new SqlCommand("sp_Print_dsaId", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@idDsa", dsaId);

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var jsonDsa = reader.IsDBNull(reader.GetOrdinal("dsa"))
                                        ? "[]"
                                        : reader.GetString(reader.GetOrdinal("dsa"));
                                    printDsa = JsonConvert.DeserializeObject<PrintDsa>(jsonDsa);
                                }
                            }
                        }

                        var pdfService = new FacturaPdfService();
                        printDsa.Cufe = cufe;
                        printDsa.CodigoQR = qrCode;
                        byte[] pdfBytes = pdfService.GenerarPdfDocumentoSoporte(printDsa, Convert.ToInt32(dsa.IdMetodoDian));
                        _logger.LogInformation("PDF de Documento Soporte generado correctamente.");

                        try
                        {
                            var facturaEmailDto = new FacturaEmailDto
                            {
                                Email = printDsa.ClienteEmail,
                                NombreCliente = printDsa.ClienteRazonSocial,
                                NumeroDocumento = apiResponse.numeroFacturaDian,
                                SubjectEmail = printDsa.SubjectEmail ?? string.Empty,
                                Total = printDsa.TotalVenta,
                                PdfAttachment = pdfBytes,
                                PdfFileName = $"DocumentoSoporte_{apiResponse.numeroFacturaDian}.pdf",
                                XmlAttachment = attachedDocumentBytes,
                                XmlFileName = $"DocumentoSoporte_{apiResponse.numeroFacturaDian}.xml",
                                FacturadorNombre = printDsa.FacturadorNombre ?? string.Empty,
                            };

                            await _emailService.SendFacturaEmailAsync(facturaEmailDto, Convert.ToInt32(dsa.IdMetodoDian));
                            _logger.LogInformation($"Email enviado exitosamente a {facturaEmailDto.Email}");
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogWarning($"Documento Soporte creado pero falló el envío de email: {emailEx.Message}");
                        }
                         */

                        /*return Ok(new
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
        }*/
    }
}