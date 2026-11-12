using ApiAstilPos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class CargosNominaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CargosNominaController> _logger;

        public CargosNominaController(IConfiguration configuration, ILogger<CargosNominaController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("cargos-nomina")]
        public async Task<IActionResult> GetCargosNomina([FromQuery] long? idCargoEmpleado = null)
        {
            _logger.LogInformation("Obteniendo lista de cargos nomina");

            try
            {
                var cargosNomina = new List<CargosNomina>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_cargosNominaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idCargoEmpleado", (object?)idCargoEmpleado ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var json = reader.IsDBNull(reader.GetOrdinal("cargosNomina"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("cargosNomina"));

                                cargosNomina = JsonConvert.DeserializeObject<List<CargosNomina>>(json) ?? new List<CargosNomina>();
                            }
                        }
                    }
                }

                _logger.LogInformation($"Cargos nomina obtenidos: {cargosNomina.Count}");
                return Ok(cargosNomina);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cargos nomina");
                return StatusCode(500, new { error = true, idCargoEmpleado = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}