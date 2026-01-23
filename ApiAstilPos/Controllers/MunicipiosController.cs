using azureFunctionPos.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MunicipiosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MunicipiosController> _logger;

        public MunicipiosController(IConfiguration configuration, ILogger<MunicipiosController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetMunicipios()
        {
            _logger.LogInformation("Obteniendo lista de municipios");

            try
            {
                var municipios = new List<DepartamentoMunicipio>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_departamentoMunicipios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonDepartamentoMunicipios = reader.IsDBNull(reader.GetOrdinal("departamentoMunicipios"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("departamentoMunicipios"));
                                municipios = JsonConvert.DeserializeObject<List<DepartamentoMunicipio>>(jsonDepartamentoMunicipios);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Municipios obtenidos: {municipios.Count}");
                return Ok(municipios);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener municipios: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}