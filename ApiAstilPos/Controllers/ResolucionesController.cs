using ApiAstilPos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text.Json;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class ResolucionesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ResolucionesController> _logger;

        public ResolucionesController(IConfiguration configuration, ILogger<ResolucionesController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("resoluciones")]
        public async Task<IActionResult> GetResoluciones()
        {
            try
            {
                var resoluciones = new List<resoluciones>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_resolucionesFacturacion", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonResoluciones = reader.IsDBNull(reader.GetOrdinal("resoluciones"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("resoluciones"));

                                resoluciones = JsonConvert.DeserializeObject<List<resoluciones>>(jsonResoluciones)
                                               ?? new List<resoluciones>();
                            }
                        }
                    }
                }
                return Ok(resoluciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener resoluciones");
                return StatusCode(500, "Internal server error");
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
                    using (var command = new SqlCommand("sp_Create_resolucionesFacturacion", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@resolucionesFacturacion", requestBody ?? (object)DBNull.Value);

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
                    using (var command = new SqlCommand("sp_Update_resolucionesFacturacion", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@resolucionesFacturacion", requestBody ?? (object)DBNull.Value);

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
                    using (var command = new SqlCommand("sp_Delete_resolucionesFacturacion", connection))
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