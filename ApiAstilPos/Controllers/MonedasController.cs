using ApiAstilPos.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/monedas")]
    public class MonedasController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MonedasController> _logger;

        public MonedasController(IConfiguration configuration, ILogger<MonedasController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetMonedas([FromQuery] long? idMoneda = null)
        {
            _logger.LogInformation("Obteniendo lista de monedas");

            try
            {
                var monedas = new List<Moneda>();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_monedasId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Se pasa el parámetro idMoneda al Stored Procedure
                        command.Parameters.AddWithValue("@idMoneda", (object)idMoneda ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int ordinal = reader.GetOrdinal("monedas");
                                if (!reader.IsDBNull(ordinal))
                                {
                                    string jsonResult = reader.GetString(ordinal);

                                    var options = new JsonSerializerOptions
                                    {
                                        PropertyNameCaseInsensitive = true
                                    };

                                    monedas = JsonSerializer.Deserialize<List<Moneda>>(jsonResult, options) ?? new List<Moneda>();
                                }
                            }
                        }
                    }
                }

                return Ok(monedas);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener las monedas: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}