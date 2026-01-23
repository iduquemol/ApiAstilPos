using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/tiposregimen")]
    public class TipoRegimenController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TipoRegimenController> _logger;

        public TipoRegimenController(IConfiguration configuration, ILogger<TipoRegimenController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetTiposRegimen()
        {
            _logger.LogInformation("Obteniendo lista de tipos de regimen");

            try
            {
                var tiposRegimen = new List<TipoRegimen>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposRegimen", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonTiposRegimen = reader.IsDBNull(reader.GetOrdinal("tiposRegimen"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("tiposRegimen"));
                                tiposRegimen = JsonConvert.DeserializeObject<List<TipoRegimen>>(jsonTiposRegimen);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Tipos de regimen obtenidos: {tiposRegimen.Count}");
                return Ok(tiposRegimen);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tipos de regimen: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}