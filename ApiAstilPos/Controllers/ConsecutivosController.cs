using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using ApiAstilPos.Models;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsecutivosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConsecutivosController> _logger;

        public ConsecutivosController(IConfiguration configuration, ILogger<ConsecutivosController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("consecutivos")]
        public async Task<IActionResult> GetConsecutivos()
        {
            _logger.LogInformation("Obteniendo lista de consecutivos");

            try
            {
                var consecutivos = new List<Consecutivo>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_consecutivos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonConsecutivos = reader.IsDBNull(reader.GetOrdinal("consecutivos"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("consecutivos"));
                                consecutivos = JsonConvert.DeserializeObject<List<Consecutivo>>(jsonConsecutivos) ?? new List<Consecutivo>();
                            }
                        }
                    }
                }

                return Ok(consecutivos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener consecutivos: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}