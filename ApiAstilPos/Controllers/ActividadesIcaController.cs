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
    public class ActividadesIcaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ActividadesIcaController> _logger;

        public ActividadesIcaController(IConfiguration configuration, ILogger<ActividadesIcaController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
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

        [HttpGet("actividadesIca")]
        public async Task<IActionResult> GetActividadesIca()
        {
            _logger.LogInformation("Obteniendo lista de actividades ICA");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_actividadesIcaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("actividadesIca");

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

                var actividades = JsonConvert.DeserializeObject<List<ActividadesIca>>(jsonCompleto) ?? new List<ActividadesIca>();

                _logger.LogInformation($"Actividades ICA obtenidas: {actividades.Count}");
                return Ok(actividades);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener actividades ICA: {ex.Message}");
                return StatusCode(500, new { error = true, idActividadIca = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("actividadesIca")]
        public async Task<IActionResult> CreateActividadIca([FromBody] JsonElement actividadesIcaJson)
        {
            _logger.LogInformation("Creando una nueva actividad ICA");

            try
            {
                string requestBody = actividadesIcaJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_actividadesIca", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@actividadesIca", requestBody ?? (object)DBNull.Value);

                        var idActividadIcaParam = new SqlParameter("@idActividadIca", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idActividadIcaParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idActividadIcaParam.Value != DBNull.Value ? Convert.ToInt64(idActividadIcaParam.Value) : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Actividad ICA creada correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al crear actividad ICA: {mensajeTexto}");
                            return BadRequest(new { error = true, idActividadIca = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Actividad ICA creada con ID: {idRetornado}");
                        return Ok(new { error = false, idActividadIca = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear actividad ICA: {ex.Message}");
                return StatusCode(500, new { error = true, idActividadIca = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPut("actividadesIca")]
        public async Task<IActionResult> UpdateActividadIca([FromBody] JsonElement actividadesIcaJson)
        {
            _logger.LogInformation("Actualizando una actividad ICA");

            try
            {
                string requestBody = actividadesIcaJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_actividadesIca", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@actividadesIca", requestBody ?? (object)DBNull.Value);

                        var idActividadIcaParam = new SqlParameter("@idActividadIca", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idActividadIcaParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idActividadIcaParam.Value != DBNull.Value ? Convert.ToInt64(idActividadIcaParam.Value) : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Actividad ICA actualizada correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al actualizar actividad ICA: {mensajeTexto}");
                            return BadRequest(new { error = true, idActividadIca = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Actividad ICA con ID {idRetornado} actualizada correctamente.");
                        return Ok(new { error = false, idActividadIca = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar actividad ICA: {ex.Message}");
                return StatusCode(500, new { error = true, idActividadIca = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpDelete("actividadesIca/{id}")]
        public async Task<IActionResult> DeleteActividadIca(long id)
        {
            _logger.LogInformation($"Intentando eliminar la actividad ICA con ID: {id}");

            try
            {
                string requestBody = JsonConvert.SerializeObject(new { idActividadIca = id });

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_actividadesIca", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@actividadesIca", requestBody);

                        var idActividadIcaParam = new SqlParameter("@idActividadIca", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idActividadIcaParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idActividadIcaParam.Value != DBNull.Value ? Convert.ToInt64(idActividadIcaParam.Value) : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Actividad ICA eliminada correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al eliminar actividad ICA: {mensajeTexto}");
                            return BadRequest(new { error = true, idActividadIca = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Actividad ICA con ID {idRetornado} eliminada exitosamente.");
                        return Ok(new { error = false, idActividadIca = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar actividad ICA: {ex.Message}");
                return StatusCode(500, new { error = true, idActividadIca = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}