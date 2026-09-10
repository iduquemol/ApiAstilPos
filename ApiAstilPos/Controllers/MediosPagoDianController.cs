using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class MediosPagoDianController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MediosPagoDianController> _logger;

        public MediosPagoDianController(IConfiguration configuration, ILogger<MediosPagoDianController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("mediospagodian")]
        public async Task<IActionResult> GetMediosPagoDian()
        {
            _logger.LogInformation("Obteniendo lista de medios de pago DIAN");

            try
            {
                var listaMediosDian = new List<MedioPagoDian>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_mediosPagoDianId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var ordinal = reader.GetOrdinal("mediosPagoDian");
                                var jsonMediosDian = reader.IsDBNull(ordinal)
                                    ? "[]"
                                    : reader.GetString(ordinal);

                                listaMediosDian = JsonConvert.DeserializeObject<List<MedioPagoDian>>(jsonMediosDian)
                                                  ?? new List<MedioPagoDian>();
                            }
                        }
                    }
                }

                return Ok(listaMediosDian);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener medios de pago DIAN: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}