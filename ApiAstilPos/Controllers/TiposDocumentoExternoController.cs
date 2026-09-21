using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Text;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class TiposDocumentoExternoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TiposDocumentoExternoController> _logger;

        public TiposDocumentoExternoController(IConfiguration configuration, ILogger<TiposDocumentoExternoController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString") ?? string.Empty;
        }

        /// <summary>
        /// Método auxiliar para extraer el mensaje formateado desde el output del Stored Procedure.
        /// </summary>
        private string ExtraerMensajeDb(string mensajeRaw, string mensajePorDefecto)
        {
            if (string.IsNullOrWhiteSpace(mensajeRaw))
                return mensajePorDefecto;

            try
            {
                var listaMensajes = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(mensajeRaw);
                if (listaMensajes != null && listaMensajes.Count > 0 && listaMensajes[0].ContainsKey("mensaje"))
                {
                    return listaMensajes[0]["mensaje"]?.ToString() ?? mensajePorDefecto;
                }
            }
            catch
            {
                return mensajeRaw;
            }

            return mensajePorDefecto;
        }

        [HttpGet("tiposdocumentoexterno")]
        public async Task<IActionResult> GetTiposDocumentoExterno()
        {
            _logger.LogInformation("Obteniendo lista de tipos de documento externo");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposDocumentoExternoId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("tiposDocumentoExterno");

                            while (await reader.ReadAsync())
                            {
                                if (!reader.IsDBNull(ordinal))
                                {
                                    jsonBuilder.Append(reader.GetString(ordinal));
                                }
                            }
                        }
                    }
                }

                string jsonCompleto = jsonBuilder.ToString();

                if (string.IsNullOrWhiteSpace(jsonCompleto))
                {
                    jsonCompleto = "[]";
                }

                var tiposDocumento = JsonConvert.DeserializeObject<List<TipoDocumentoExterno>>(jsonCompleto) ?? new List<TipoDocumentoExterno>();

                _logger.LogInformation($"Tipos de documento externo obtenidos: {tiposDocumento.Count}");
                return Ok(tiposDocumento);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tipos de documento externo: {ex.Message}");
                return StatusCode(500, new { error = true, idTipoDocumentoExterno = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("tiposdocumentoexterno")]
        public async Task<IActionResult> CreateTipoDocumentoExterno([FromBody] JsonElement tiposDocumentoJson)
        {
            _logger.LogInformation("Creando un nuevo tipo de documento externo");

            try
            {
                string requestBody = tiposDocumentoJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_tiposDocumentoExterno", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@tiposDocumentoExterno", requestBody ?? (object)DBNull.Value);

                        var idTipoDocumentoExternoParam = new SqlParameter("@idTipoDocumentoExterno", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idTipoDocumentoExternoParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idTipoDocumentoExternoParam.Value != DBNull.Value ? Convert.ToInt64(idTipoDocumentoExternoParam.Value) : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Tipo de documento externo creado correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al crear tipo de documento externo: {mensajeTexto}");
                            return BadRequest(new { error = true, idTipoDocumentoExterno = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Tipo de documento externo creado con ID: {idRetornado}");
                        return Ok(new { error = false, idTipoDocumentoExterno = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear tipo de documento externo: {ex.Message}");
                return StatusCode(500, new { error = true, idTipoDocumentoExterno = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPut("tiposdocumentoexterno")]
        public async Task<IActionResult> UpdateTipoDocumentoExterno([FromBody] JsonElement tiposDocumentoJson)
        {
            _logger.LogInformation("Actualizando un tipo de documento externo");

            try
            {
                string requestBody = tiposDocumentoJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_tiposDocumentoExterno", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@tiposDocumentoExterno", requestBody ?? (object)DBNull.Value);

                        var idTipoDocumentoExternoParam = new SqlParameter("@idTipoDocumentoExterno", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idTipoDocumentoExternoParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idTipoDocumentoExternoParam.Value != DBNull.Value ? Convert.ToInt64(idTipoDocumentoExternoParam.Value) : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Tipo de documento externo actualizado correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al actualizar tipo de documento externo: {mensajeTexto}");
                            return BadRequest(new { error = true, idTipoDocumentoExterno = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Tipo de documento externo con ID {idRetornado} actualizado correctamente.");
                        return Ok(new { error = false, idTipoDocumentoExterno = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar tipo de documento externo: {ex.Message}");
                return StatusCode(500, new { error = true, idTipoDocumentoExterno = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        public class DeleteTipoDocumentoRequest
        {
            public long idTipoDocumentoExterno { get; set; }
        }

        [HttpDelete("tiposdocumentoexterno")]
        public async Task<IActionResult> DeleteTipoDocumentoExterno([FromBody] DeleteTipoDocumentoRequest request)
        {
            _logger.LogInformation($"Intentando eliminar el tipo de documento externo con ID: {request?.idTipoDocumentoExterno}");

            if (request == null || request.idTipoDocumentoExterno <= 0)
            {
                return BadRequest(new { error = true, idTipoDocumentoExterno = 0, mensaje = "ID de tipo de documento no válido." });
            }

            try
            {
                // Se envía la estructura JSON que espera la función de la BD
                string requestBody = JsonConvert.SerializeObject(new[] { new { idTipoDocumentoExterno = request.idTipoDocumentoExterno } });

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_tiposDocumentoExterno", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@tiposDocumentoExterno", requestBody);

                        var idTipoDocumentoExternoParam = new SqlParameter("@idTipoDocumentoExterno", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idTipoDocumentoExternoParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idTipoDocumentoExternoParam.Value != DBNull.Value ? Convert.ToInt64(idTipoDocumentoExternoParam.Value) : request.idTipoDocumentoExterno;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Tipo de documento externo eliminado correctamente");

                        if (tieneError)
                        {
                            _logger.LogWarning($"Error al eliminar tipo de documento externo: {mensajeTexto}");
                            return BadRequest(new { error = true, idTipoDocumentoExterno = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Tipo de documento externo con ID {idRetornado} eliminado exitosamente.");
                        return Ok(new { error = false, idTipoDocumentoExterno = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar tipo de documento externo: {ex.Message}");
                return StatusCode(500, new { error = true, idTipoDocumentoExterno = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}