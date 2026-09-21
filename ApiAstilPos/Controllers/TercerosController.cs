using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Text.Json;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class TercerosController : ControllerBase
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly IConfiguration _configuration;
        private readonly ILogger<TercerosController> _logger;

        private static readonly Dictionary<string, int> TipoDocumentoExternoMap = new()
        {
            { "11", 1 },  // Registro civil
            { "12", 2 },  // Tarjeta de identidad
            { "13", 3 },  // Cédula de ciudadanía
            { "21", 4 },  // Tarjeta de extranjería
            { "22", 5 },  // Cédula de extranjería
            { "31", 6 },  // NIT
            { "41", 7 },  // Pasaporte
            { "42", 8 },  // Documento de identificación extranjero
            { "47", 9 },  // PEP
            { "48", 10 }, // PPT
            { "50", 11 }, // NIT de otro país
            { "91", 12 }, // NUIP
        };

        public TercerosController(IConfiguration configuration, ILogger<TercerosController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        // Método auxiliar interno para parsear el mensaje JSON que arma la BD en @mensajeOutput
        private string ExtraerMensajeDb(string rawMensaje, string mensajeDefault)
        {
            if (string.IsNullOrWhiteSpace(rawMensaje)) return mensajeDefault;

            try
            {
                var lista = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(rawMensaje);
                if (lista != null && lista.Count > 0 && lista[0].TryGetValue("mensaje", out var msg))
                {
                    return msg?.ToString() ?? mensajeDefault;
                }
            }
            catch
            {
                return rawMensaje; // Fallback si viene en texto plano
            }

            return mensajeDefault;
        }

        [HttpGet("terceros")]
        public async Task<IActionResult> GetTerceros()
        {
            _logger.LogInformation("Obteniendo lista de terceros");

            try
            {
                var terceros = new List<Tercero>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tercerosId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonTerceros = reader.IsDBNull(reader.GetOrdinal("tercero"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("tercero"));
                                terceros = JsonConvert.DeserializeObject<List<Tercero>>(jsonTerceros);
                            }
                        }
                    }
                }

                return Ok(terceros);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener terceros: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("terceros-proveedores")]
        public async Task<IActionResult> GetTercerosProveedores()
        {
            _logger.LogInformation("Obteniendo lista de terceros proveedores");

            try
            {
                var tercerosProveedores = new List<TerceroProveedor>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_ProveedoresId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonTercerosProveedores = reader.IsDBNull(reader.GetOrdinal("terceros"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("terceros"));
                                tercerosProveedores = JsonConvert.DeserializeObject<List<TerceroProveedor>>(jsonTercerosProveedores);
                            }
                        }
                    }
                }

                return Ok(tercerosProveedores);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener terceros: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("terceros-busqueda")]
        public async Task<IActionResult> GetTercerosBusqueda([FromBody] JsonElement request)
        {
            _logger.LogInformation($"Buscando terceros");

            try
            {
                var query = request.TryGetProperty("query", out var queryElement)
                    ? queryElement.GetString()
                    : null;

                var terceros = new List<Tercero>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Search_terceros", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@searchText", query ?? (object)DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonTercero = reader.IsDBNull(reader.GetOrdinal("tercero"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("tercero"));
                                terceros = JsonConvert.DeserializeObject<List<Tercero>>(jsonTercero);
                            }
                        }
                    }
                }

                return Ok(terceros);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener terceros: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("terceros-consulta-externa")]
        public async Task<IActionResult> ConsultarTerceroExterno([FromBody] JsonElement request)
        {
            _logger.LogInformation("Consultando datos de tercero en proveedor externo");

            try
            {
                var codigoTipoDocumentoId = request.TryGetProperty("codigoTipoDocumentoId", out var codigoElement)
                    ? codigoElement.GetString()
                    : null;
                var identificationNumber = request.TryGetProperty("identificationNumber", out var idElement)
                    ? idElement.GetString()
                    : null;

                if (string.IsNullOrEmpty(codigoTipoDocumentoId) || string.IsNullOrEmpty(identificationNumber))
                {
                    return BadRequest("codigoTipoDocumentoId e identificationNumber son requeridos");
                }

                if (!TipoDocumentoExternoMap.TryGetValue(codigoTipoDocumentoId, out var typeDocumentIdentificationId))
                {
                    _logger.LogWarning($"No hay mapeo externo para codigoTipoDocumentoId: {codigoTipoDocumentoId}");
                    return BadRequest("Tipo de documento no soportado para consulta externa");
                }

                var acquirerStatusUrl = _configuration["ApiExterna:AcquirerStatusUrl"];
                var bearerToken = _configuration["ApiExterna:BearerToken"];

                if (string.IsNullOrEmpty(acquirerStatusUrl))
                {
                    _logger.LogError("URL de consulta externa de terceros no configurada");
                    return BadRequest("Configuración de API externa faltante");
                }

                var requestBody = new AcquirerStatusRequest
                {
                    environment = new AcquirerStatusEnvironment { type_environment_id = 1 },
                    type_document_identification_id = typeDocumentIdentificationId,
                    identification_number = identificationNumber
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

                _logger.LogInformation($"Consultando proveedor externo para identificación: {identificationNumber}");
                var response = await httpClient.PostAsync(acquirerStatusUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error en API externa de consulta de tercero: {response.StatusCode} - {responseContent}");
                    return BadRequest("Error al consultar el proveedor externo");
                }

                var acquirerResponse = JsonConvert.DeserializeObject<AcquirerStatusResponse>(responseContent);
                return Ok(acquirerResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al consultar tercero en proveedor externo: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("terceros")]
        public async Task<IActionResult> CreateTercero([FromBody] JsonElement tercerosJson)
        {
            _logger.LogInformation("Creando un tercero");

            try
            {
                string requestBody = tercerosJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_terceros", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@terceros", requestBody ?? (object)DBNull.Value);

                        var paramIdTercero = new SqlParameter("@idtercero", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramIdTercero);
                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync()) { }
                        }

                        long idTercero = paramIdTercero.Value != DBNull.Value ? Convert.ToInt64(paramIdTercero.Value) : 0;
                        bool errorOutput = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeRaw = paramMensaje.Value?.ToString() ?? string.Empty;

                        string mensajeFinal = ExtraerMensajeDb(mensajeRaw, errorOutput ? "No se pudo crear el tercero." : "Tercero creado correctamente.");

                        if (errorOutput || idTercero == 0)
                        {
                            _logger.LogWarning($"Error al crear tercero: {mensajeFinal}");
                            return BadRequest(new
                            {
                                error = true,
                                idTercero = 0,
                                mensaje = mensajeFinal
                            });
                        }

                        _logger.LogInformation($"Tercero creado correctamente con ID {idTercero}");
                        return Ok(new
                        {
                            error = false,
                            idTercero,
                            mensaje = mensajeFinal
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear tercero: {ex.Message}");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPut("terceros")]
        public async Task<IActionResult> UpdateTercero([FromBody] JsonElement tercerosJson)
        {
            _logger.LogInformation("Actualizando un tercero");

            try
            {
                string requestBody = tercerosJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_terceros", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@terceros", requestBody ?? (object)DBNull.Value);

                        var paramIdTercero = new SqlParameter("@idtercero", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramIdTercero);
                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync()) { }
                        }

                        long idTercero = paramIdTercero.Value != DBNull.Value ? Convert.ToInt64(paramIdTercero.Value) : 0;
                        bool errorOutput = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeRaw = paramMensaje.Value?.ToString() ?? string.Empty;

                        string mensajeFinal = ExtraerMensajeDb(mensajeRaw, errorOutput ? "No se pudo actualizar el tercero." : "Tercero actualizado correctamente.");

                        if (errorOutput || idTercero == 0)
                        {
                            _logger.LogWarning($"Error al actualizar tercero ID {idTercero}: {mensajeFinal}");
                            return BadRequest(new
                            {
                                error = true,
                                idTercero = 0,
                                mensaje = mensajeFinal
                            });
                        }

                        _logger.LogInformation($"Tercero con ID {idTercero} actualizado correctamente.");
                        return Ok(new
                        {
                            error = false,
                            idTercero,
                            mensaje = mensajeFinal
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar tercero: {ex.Message}");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpDelete("terceros/{id}")]
        public async Task<IActionResult> DeleteTercero(long id)
        {
            _logger.LogInformation($"Intentando eliminar el tercero con ID: {id}");

            try
            {
                string requestBody = JsonConvert.SerializeObject(new { idTercero = id });

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_terceros", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@terceros", requestBody);

                        var paramIdTercero = new SqlParameter("@idTercero", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramIdTercero);
                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync()) { }
                        }

                        long idTercero = paramIdTercero.Value != DBNull.Value ? Convert.ToInt64(paramIdTercero.Value) : 0;
                        bool errorOutput = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeRaw = paramMensaje.Value?.ToString() ?? string.Empty;

                        string mensajeFinal = ExtraerMensajeDb(mensajeRaw, errorOutput ? "No se pudo eliminar el tercero." : "Tercero eliminado correctamente.");

                        if (errorOutput || idTercero == 0)
                        {
                            _logger.LogWarning($"Error al eliminar tercero ID {id}: {mensajeFinal}");
                            return BadRequest(new
                            {
                                error = true,
                                idTercero = 0,
                                mensaje = mensajeFinal
                            });
                        }

                        _logger.LogInformation($"Tercero con ID {idTercero} eliminado exitosamente.");
                        return Ok(new
                        {
                            error = false,
                            idTercero,
                            mensaje = mensajeFinal
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar tercero con ID {id}: {ex.Message}");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}