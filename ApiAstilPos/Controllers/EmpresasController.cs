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
            return _configuration.GetConnectionString("SqlConnectionString")
                ?? throw new InvalidOperationException("La cadena de conexión 'SqlConnectionString' no está configurada.");
        }

        
        [HttpGet("empresas")]
        public async Task<IActionResult> GetEmpresaUnica()
        {
            _logger.LogInformation("Obteniendo información de la empresa usuaria");

            try
            {
                var empresasList = new List<empresas>();

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

                                empresasList = JsonConvert.DeserializeObject<List<empresas>>(jsonempresas) ?? new List<empresas>();
                            }
                        }
                    }
                }

                // Extraemos el primer registro existente (Registro Único)
                var empresaUnica = empresasList.FirstOrDefault();

                if (empresaUnica == null)
                {
                    // Si no existen registros aún, se puede retornar NoContent (204) o un objeto vacío
                    return Ok(null);
                }

                return Ok(empresaUnica);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la empresa");
                return StatusCode(500, new { message = "Error al obtener la empresa", error = ex.Message });
            }
        }

        
        [HttpPut("empresas")]
        public async Task<IActionResult> UpdateEmpresa([FromBody] JsonElement empresasJson)
        {
            _logger.LogInformation("Actualizando la información de la empresa");

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

                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Empresa actualizada correctamente.");
                        return Ok(new { message = "Empresa actualizada correctamente.", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la empresa");
                return StatusCode(500, new { message = "Error al actualizar la empresa", error = ex.Message });
            }
        }
    }
}