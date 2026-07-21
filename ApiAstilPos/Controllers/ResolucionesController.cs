using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class resolucionesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<resolucionesController> _logger;

        public resolucionesController(IConfiguration configuration, ILogger<resolucionesController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("resoluciones")]
        public async Task<IActionResult> Getresoluciones()
        {
            _logger.LogInformation("Obteniendo lista de Resoluciones");

            try
            {
                var resoluciones = new List<resoluciones>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_resoluciones", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonresoluciones = reader.IsDBNull(reader.GetOrdinal("resoluciones"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("resoluciones"));
                                resoluciones = JsonConvert.DeserializeObject<List<resoluciones>>(jsonresoluciones);
                            }
                        }
                    }
                }

                return Ok(resoluciones);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener resoluciones: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("resoluciones")]
        public async Task<IActionResult> Createresoluciones([FromBody] JsonElement resolucionesJson)
        {
            _logger.LogInformation("Creando una nueva resolucion");

            try
            {
                string requestBody = resolucionesJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Insert_resolucionesId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idResoluciones", requestBody ?? (object)DBNull.Value);

                        // Ejecutar el SP
                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Resolución creada correctamente.");
                        return Ok(new { message = "Resolución creada correctamente.", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear resolución: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPut("resoluciones")]
        public async Task<IActionResult> Updateresoluciones([FromBody] JsonElement resolucionesJson)
        {
            _logger.LogInformation("Actualizando una resolución");

            try
            {
                string requestBody = resolucionesJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_ResolucionesId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@resoluciones", requestBody ?? (object)DBNull.Value);

                        // Ejecutar el SP
                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Resolución actualizada correctamente.");
                        return Ok(new { message = "Resolución actualizada correctamente.", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar resolución: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete("resoluciones")]
        public async Task<IActionResult> Deleteresoluciones([FromBody] JObject request)
        {
            _logger.LogInformation("Borrando resolución");

            try
            {
                var idResoluciones = request["idResoluciones"]?.Value<long>() ?? 0;
                _logger.LogInformation($"ID resolución a borrar: {idResoluciones}");
                var idresolucionesBorrado = 0;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_ResolucionesId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idResoluciones", idResoluciones);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                idresolucionesBorrado = reader.IsDBNull(reader.GetOrdinal("idResoluciones"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("idResoluciones"));
                            }
                        }

                        _logger.LogInformation("Resolución borrada correctamente.");
                        return Ok(new { message = "Resolución borrada correctamente.", idresolucionesBorrado = idresolucionesBorrado });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al borrar resolución: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}