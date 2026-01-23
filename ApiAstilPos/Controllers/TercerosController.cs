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
    public class TercerosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TercerosController> _logger;

        public TercerosController(IConfiguration configuration, ILogger<TercerosController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
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
                    using (var command = new SqlCommand("sp_Read_terceros", connection))
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

        [HttpPost("terceros")]
        public async Task<IActionResult> CreateTercero([FromBody] JsonElement tercerosJson)
        {
            _logger.LogInformation("Creando un nuevo tercero");

            try
            {
                string requestBody = tercerosJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_terceros", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@terceros", requestBody ?? (object)DBNull.Value);

                        // Ejecutar el SP
                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Tercero creado correctamente.");
                        return Ok(new { message = "Tercero creado correctamente", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear tercero: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPut("terceros")]
        public async Task<IActionResult> UpdateTercero([FromBody] JsonElement tercerosJson)
        {
            _logger.LogInformation("Actualizando un tercero");

            try
            {
                string requestBody = tercerosJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_terceros", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@terceros", requestBody ?? (object)DBNull.Value);

                        // Ejecutar el SP
                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Tercero actualizado correctamente.");
                        return Ok(new { message = "Tercero actualizado correctamente", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar tercero: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTercero([FromBody] JObject request)
        {
            _logger.LogInformation("Borrando un tercero");

            try
            {
                var idTercero = request["idtercero"]?.Value<long>() ?? 0;
                _logger.LogInformation($"ID Tercero a borrar: {idTercero}");
                var idTerceroBorrado = 0;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_tercerosId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idTercero", idTercero);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                idTerceroBorrado = reader.IsDBNull(reader.GetOrdinal("idTercero"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("idTercero"));
                            }
                        }

                        _logger.LogInformation("Tercero borrado correctamente.");
                        return Ok(new { message = "Tercero borrado correctamente", idTerceroBorrado = idTerceroBorrado });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al borrar tercero: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}