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
                    using (var command = new SqlCommand("sp_Read_actividadesIca", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonActividades = reader.IsDBNull(reader.GetOrdinal("actividadesIca"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("actividadesIca"));

                                actividades = JsonConvert.DeserializeObject<List<ActividadesIca>>(jsonActividades)
                                              ?? new List<ActividadesIca>();
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

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var idRetornado = reader.GetInt64(reader.GetOrdinal("idActividadIca"));

                                if (idRetornado == 0)
                                {
                                    _logger.LogWarning("No se pudo crear la actividad ICA. Posiblemente ya existe el código.");
                                    return BadRequest("No se pudo crear la actividad ICA. Verifique si el código de la actividad ya existe.");
                                }

                                _logger.LogInformation($"Actividad ICA creada con ID: {idRetornado}");
                                return Ok(new { message = "Actividad ICA creada correctamente", idActividadIca = idRetornado });
                            }
                        }

                        return BadRequest("No se recibió respuesta de confirmación de la base de datos.");
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

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var idRetornado = reader.GetInt64(reader.GetOrdinal("idActividadIca"));

                                _logger.LogInformation($"Actividad ICA con ID {idRetornado} actualizada correctamente.");
                                return Ok(new { message = "Actividad ICA actualizada correctamente", idActividadIca = idRetornado });
                            }
                        }

                        return BadRequest("No se recibió respuesta de confirmación de la base de datos.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar actividad ICA: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete("actividadesIca/{id}")]
        public async Task<IActionResult> DeleteActividadIca(long id)
        {
            _logger.LogInformation($"Intentando eliminar la actividad ICA con ID: {id}");

            try
            {
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_actividadesIca", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idActividadIca", id);

                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation($"Actividad ICA con ID {id} eliminada exitosamente.");
                        return Ok(new { message = "Actividad ICA eliminada correctamente", idActividadIca = id });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar actividad ICA con ID {id}: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}