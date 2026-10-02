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

        [HttpPost("obtener-documento-soporte")]
        public async Task<IActionResult> GetByIdDocumentoSoporteId([FromBody] JsonElement request)
        {
            _logger.LogInformation("Procesando solicitud para obtener documento soporte por ID");

            try
            {
                // 1. Usar GetInt64() para soportar IDs grandes como 82161 sin desbordar Int16
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
                                    // 2. Si el SP devuelve un array JSON, deserializa el primer elemento o la lista
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
    }
}