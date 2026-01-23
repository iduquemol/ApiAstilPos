using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendedorController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VendedorController> _logger;

        public VendedorController(IConfiguration configuration, ILogger<VendedorController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetVendedores()
        {
            _logger.LogInformation("Obteniendo lista de vendedores");

            try
            {
                var vendedores = new List<Vendedor>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_vendedores", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonVendedores = reader.IsDBNull(reader.GetOrdinal("vendedores"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("vendedores"));
                                vendedores = JsonConvert.DeserializeObject<List<Vendedor>>(jsonVendedores);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Vendedores obtenidos: {vendedores.Count}");
                return Ok(vendedores);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener vendedores: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}