using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class TributoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TributoController> _logger;

        public TributoController(IConfiguration configuration, ILogger<TributoController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("tributos")]
        public async Task<IActionResult> GetTributos()
        {
            _logger.LogInformation("Obteniendo lista de tributos");

            try
            {
                var tributos = new List<Tributo>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tributosId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var ordinal = reader.GetOrdinal("tributos");
                                var jsonTributos = reader.IsDBNull(ordinal)
                                    ? "[]"
                                    : reader.GetString(ordinal);

                                tributos = JsonConvert.DeserializeObject<List<Tributo>>(jsonTributos)
                                           ?? new List<Tributo>();
                            }
                        }
                    }
                }

                _logger.LogInformation($"Tributos obtenidos: {tributos.Count}");
                return Ok(tributos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tributos: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("tarifastributo")]
        public async Task<IActionResult> GetTarifasTributo()
        {
            _logger.LogInformation("Obteniendo lista de tarifas por tributo");

            try
            {
                var tributosTarifas = new List<TributoTarifa>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tarifaTributoId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var ordinal = reader.GetOrdinal("tarifaTributo");
                                var jsonTarifasTributo = reader.IsDBNull(ordinal)
                                    ? "[]"
                                    : reader.GetString(ordinal);

                                tributosTarifas = JsonConvert.DeserializeObject<List<TributoTarifa>>(jsonTarifasTributo)
                                                  ?? new List<TributoTarifa>();
                            }
                        }
                    }
                }

                _logger.LogInformation($"Tarifas Tributo obtenidas: {tributosTarifas.Count}");
                return Ok(tributosTarifas);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tarifas por tributo: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}