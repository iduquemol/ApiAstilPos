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
    public class mediosPagoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<mediosPagoController> _logger;

        public mediosPagoController(IConfiguration configuration, ILogger<mediosPagoController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("mediosPago")]
        public async Task<IActionResult> GetmediosPago()
        {
            _logger.LogInformation("Obteniendo lista de Medios de Pago");

            try
            {
                var mediosPago = new List<mediosPago>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_mediosPago", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonmediosPago = reader.IsDBNull(reader.GetOrdinal("mediosPago"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("mediosPago"));
                                mediosPago = JsonConvert.DeserializeObject<List<mediosPago>>(jsonmediosPago);
                            }
                        }
                    }
                }

                return Ok(mediosPago);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener Medios de Pago: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}