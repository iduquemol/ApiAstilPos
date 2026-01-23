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
                    using (var command = new SqlCommand("sp_Read_tributos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tributos.Add(new Tributo
                                {
                                    IdTributo = reader.GetInt64(reader.GetOrdinal("idTributo")),
                                    CodigoTributo = reader.IsDBNull(reader.GetOrdinal("codigoTributo"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("codigoTributo")),
                                    NombreTributo = reader.IsDBNull(reader.GetOrdinal("nombreTributo"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("nombreTributo")),
                                    DescripcionTributo = reader.IsDBNull(reader.GetOrdinal("descripcionTributo"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("descripcionTributo")),
                                });
                            }
                        }
                    }
                }

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
                var tributos = new List<TributoTarifa>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_TarifasTributo", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonTarifasTributo = reader.IsDBNull(reader.GetOrdinal("tarifasTributo"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("tarifasTributo"));
                                tributos = JsonConvert.DeserializeObject<List<TributoTarifa>>(jsonTarifasTributo);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Tarifas Tributo obtenidos: {tributos.Count}");
                return Ok(tributos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tarifas por tributo: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}