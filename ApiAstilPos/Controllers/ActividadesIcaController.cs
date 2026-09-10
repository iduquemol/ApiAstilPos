using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;

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

        [HttpGet("actividadesIca")]
        public async Task<IActionResult> GetActividadesIca()
        {
            _logger.LogInformation("Obteniendo lista de actividades ICA");

            try
            {
                var actividades = new List<ActividadesIca>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_actividadesIcaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonActividades = reader.IsDBNull(reader.GetOrdinal("actividadesIca"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("actividadesIca"));

                                // CORRECCIÓN: Acumular/concatenar en lugar de reasignar
                                var listaTemporal = JsonConvert.DeserializeObject<List<ActividadesIca>>(jsonActividades);
                                if (listaTemporal != null)
                                {
                                    actividades.AddRange(listaTemporal);
                                }
                            }
                        }
                    }
                }
                _logger.LogInformation($"Actividades ICA obtenidas: {actividades.Count}");
                return Ok(actividades);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener actividades ICA: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("actividadesIca")]
        public async Task<IActionResult> CreateActividadIca([FromBody] JsonElement actividadesIcaJson)
        {
            _logger.LogInformation("Creando una nueva actividad ICA");

            try
            {
                string requestBody = actividadesIcaJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_actividadesIca", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@actividadesIca", requestBody ?? (object)DBNull.Value);

                        var idActividadIcaParam = new SqlParameter("@idActividadIca", SqlDbType.BigInt)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };

                        command.Parameters.Add(idActividadIcaParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idActividadIcaParam.Value != DBNull.Value ? (long)idActividadIcaParam.Value : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && (bool)errorOutputParam.Value;
                        string mensaje = mensajeOutputParam.Value != DBNull.Value ? mensajeOutputParam.Value.ToString() : string.Empty;

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al crear actividad ICA: {mensaje}");
                            return BadRequest(string.IsNullOrEmpty(mensaje) ? "No se pudo crear la actividad ICA." : mensaje);
                        }

                        _logger.LogInformation($"Actividad ICA creada con ID: {idRetornado}");
                        return Ok(new { message = string.IsNullOrEmpty(mensaje) ? "Actividad ICA creada correctamente" : mensaje, idActividadIca = idRetornado });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear actividad ICA: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPut("actividadesIca")]
        public async Task<IActionResult> UpdateActividadIca([FromBody] JsonElement actividadesIcaJson)
        {
            _logger.LogInformation("Actualizando una actividad ICA");

            try
            {
                string requestBody = actividadesIcaJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_actividadesIca", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@actividadesIca", requestBody ?? (object)DBNull.Value);

                        var idActividadIcaParam = new SqlParameter("@idActividadIca", SqlDbType.BigInt)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };

                        command.Parameters.Add(idActividadIcaParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idActividadIcaParam.Value != DBNull.Value ? (long)idActividadIcaParam.Value : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && (bool)errorOutputParam.Value;
                        string mensaje = mensajeOutputParam.Value != DBNull.Value ? mensajeOutputParam.Value.ToString() : string.Empty;

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al actualizar actividad ICA: {mensaje}");
                            return BadRequest(string.IsNullOrEmpty(mensaje) ? "No se pudo actualizar la actividad ICA." : mensaje);
                        }

                        _logger.LogInformation($"Actividad ICA con ID {idRetornado} actualizada correctamente.");
                        return Ok(new { message = string.IsNullOrEmpty(mensaje) ? "Actividad ICA actualizada correctamente" : mensaje, idActividadIca = idRetornado });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar actividad ICA: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete("actividadesIca")]
        public async Task<IActionResult> DeleteActividadIca([FromBody] JsonElement actividadesIcaJson)
        {
            _logger.LogInformation("Intentando eliminar la actividad ICA");

            try
            {
                string requestBody = actividadesIcaJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_actividadesIca", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@actividadesIca", requestBody ?? (object)DBNull.Value);

                        var idActividadIcaParam = new SqlParameter("@idActividadIca", SqlDbType.BigInt)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };

                        command.Parameters.Add(idActividadIcaParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idActividadIcaParam.Value != DBNull.Value ? (long)idActividadIcaParam.Value : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && (bool)errorOutputParam.Value;
                        string mensaje = mensajeOutputParam.Value != DBNull.Value ? mensajeOutputParam.Value.ToString() : string.Empty;

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al eliminar actividad ICA: {mensaje}");
                            return BadRequest(string.IsNullOrEmpty(mensaje) ? "No se pudo eliminar la actividad ICA." : mensaje);
                        }

                        _logger.LogInformation($"Actividad ICA con ID {idRetornado} eliminada exitosamente.");
                        return Ok(new { message = "Actividad ICA eliminada correctamente", idActividadIca = idRetornado });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar actividad ICA: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}