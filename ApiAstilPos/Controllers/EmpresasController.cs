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
    public class empresasController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<empresasController> _logger;

        public empresasController(IConfiguration configuration, ILogger<empresasController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("empresas")]
        public async Task<IActionResult> GettiposDocumentoExterno()
        {
            _logger.LogInformation("Obteniendo lista de empresas");

            try
            {
                var empresas = new List<empresas>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_empresas", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonempresas = reader.IsDBNull(reader.GetOrdinal("empresas"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("empresas"));
                                empresas = JsonConvert.DeserializeObject<List<empresas>>(jsonempresas);
                            }
                        }
                    }
                }

                return Ok(empresas);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener empresa: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("empresas")]
        public async Task<IActionResult> Createempresas([FromBody] JsonElement empresasJson)
        {
            _logger.LogInformation("Creando una nuevo empresa");

            try
            {
                string requestBody = empresasJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_empresas", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@empresas", requestBody ?? (object)DBNull.Value);

                        // Ejecutar el SP
                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Empresa creada correctamente.");
                        return Ok(new { message = "Empresa creada correctamente.", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear empresa: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPut("empresas")]
        public async Task<IActionResult> Updateempresas([FromBody] JsonElement empresasJson)
        {
            _logger.LogInformation("Actualizando una empresa");

            try
            {
                string requestBody = empresasJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_empresasId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@empresas", requestBody ?? (object)DBNull.Value);

                        // Ejecutar el SP
                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Empresa actualizada correctamente.");
                        return Ok(new { message = "Empresa actualizada correctamente.", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar empresa: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete("empresas")]
        public async Task<IActionResult> Deleteempresas([FromBody] JObject request)
        {
            _logger.LogInformation("Borrando una empresa");

            try
            {
                var idempresas = request["idempresas"]?.Value<long>() ?? 0;
                _logger.LogInformation($"ID empresa a borrar: {idempresas}");
                var idempresasBorrado = 0;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_empresasId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idempresas", idempresas);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                idempresasBorrado = reader.IsDBNull(reader.GetOrdinal("idempresas"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("idempresas"));
                            }
                        }

                        _logger.LogInformation("Empresa borrada correctamente.");
                        return Ok(new { message = "Empresa borrada correctamente.", idempresasBorrado = idempresasBorrado });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al borrar empresa: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}