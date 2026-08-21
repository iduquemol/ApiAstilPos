using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using ApiAstilPos.Models;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormasPagoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FormasPagoController> _logger;

        public FormasPagoController(IConfiguration configuration, ILogger<FormasPagoController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("formasPago")]
        public async Task<IActionResult> GetFormasPago()
        {
            _logger.LogInformation("Obteniendo lista de formas de pago");

            try
            {
                var formasPago = new List<FormaPago>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_formasPago", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonFormasPago = reader.IsDBNull(reader.GetOrdinal("formasPago"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("formasPago"));
                                formasPago = JsonConvert.DeserializeObject<List<FormaPago>>(jsonFormasPago) ?? new List<FormaPago>();
                            }
                        }
                    }
                }

                return Ok(formasPago);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener formas de pago: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}