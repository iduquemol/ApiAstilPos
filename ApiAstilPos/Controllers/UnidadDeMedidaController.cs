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
    public class UnidadDeMedidaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UnidadDeMedidaController> _logger;

        public UnidadDeMedidaController(IConfiguration configuration, ILogger<UnidadDeMedidaController> logger)
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

        [HttpGet("unidadesdemedida")]
        public async Task<IActionResult> GetUnidadesMedidaDian()
        {
            _logger.LogInformation("Obteniendo lista de unidades de medida");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_unidadesMedidaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("unidadesMedida");

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

                var unidadesDeMedida = JsonConvert.DeserializeObject<List<UnidadDeMedida>>(jsonCompleto) ?? new List<UnidadDeMedida>();

                _logger.LogInformation($"Unidades de medida obtenidas: {unidadesDeMedida.Count}");
                return Ok(unidadesDeMedida);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener unidades de medida: {ex.Message}");
                return StatusCode(500, new { error = true, idUnidadMedida = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("unidadesdemedida")]
        public async Task<IActionResult> CreateUnidadesDeMedida([FromBody] JsonElement unidadesDeMedidaJson)
        {
            _logger.LogInformation("Creando una nueva unidad de medida");

            try
            {
                string requestBody = unidadesDeMedidaJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_unidadesMedida", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@unidadesMedida", requestBody ?? (object)DBNull.Value);

                        var idUnidadMedidaParam = new SqlParameter("@idUnidadMedida", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idUnidadMedidaParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idUnidadMedidaParam.Value != DBNull.Value ? Convert.ToInt64(idUnidadMedidaParam.Value) : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Unidad de medida creada correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al crear unidad de medida: {mensajeTexto}");
                            return BadRequest(new { error = true, idUnidadMedida = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Unidad de medida creada con ID: {idRetornado}");
                        return Ok(new { error = false, idUnidadMedida = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear unidad de medida: {ex.Message}");
                return StatusCode(500, new { error = true, idUnidadMedida = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPut("unidadesdemedida")]
        public async Task<IActionResult> UpdateUnidadesDeMedida([FromBody] JsonElement unidadesDeMedidaJson)
        {
            _logger.LogInformation("Actualizando una unidad de medida");

            try
            {
                string requestBody = unidadesDeMedidaJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_unidadesMedida", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@unidadesMedida", requestBody ?? (object)DBNull.Value);

                        var idUnidadMedidaParam = new SqlParameter("@idUnidadMedida", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idUnidadMedidaParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idUnidadMedidaParam.Value != DBNull.Value ? Convert.ToInt64(idUnidadMedidaParam.Value) : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Unidad de medida actualizada correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al actualizar unidad de medida: {mensajeTexto}");
                            return BadRequest(new { error = true, idUnidadMedida = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Unidad de medida con ID {idRetornado} actualizada correctamente.");
                        return Ok(new { error = false, idUnidadMedida = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar unidad de medida: {ex.Message}");
                return StatusCode(500, new { error = true, idUnidadMedida = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpDelete("unidadesdemedida/{id}")]
        public async Task<IActionResult> DeleteUnidadesDeMedida(long id)
        {
            _logger.LogInformation($"Intentando eliminar la unidad de medida con ID: {id}");

            try
            {
                string requestBody = JsonConvert.SerializeObject(new { idUnidadMedida = id });

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_unidadesMedida", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@unidadesMedida", requestBody);

                        var idUnidadMedidaParam = new SqlParameter("@idUnidadMedida", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idUnidadMedidaParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idUnidadMedidaParam.Value != DBNull.Value ? Convert.ToInt64(idUnidadMedidaParam.Value) : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Unidad de medida eliminada correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al eliminar unidad de medida: {mensajeTexto}");
                            return BadRequest(new { error = true, idUnidadMedida = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Unidad de medida con ID {idRetornado} eliminada exitosamente.");
                        return Ok(new { error = false, idUnidadMedida = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar unidad de medida: {ex.Message}");
                return StatusCode(500, new { error = true, idUnidadMedida = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}