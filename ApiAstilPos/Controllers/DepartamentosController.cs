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
    public class DepartamentosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DepartamentosController> _logger;

        public DepartamentosController(IConfiguration configuration, ILogger<DepartamentosController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartamentos()
        {
            _logger.LogInformation("Obteniendo lista de departamentos");

            try
            {
                var departamentos = new List<Departamento>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_departamentos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonDepartamentos = reader.IsDBNull(reader.GetOrdinal("departamentos"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("departamentos"));
                                departamentos = JsonConvert.DeserializeObject<List<Departamento>>(jsonDepartamentos);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Departamentos obtenidos: {departamentos.Count}");
                return Ok(departamentos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener departamentos: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}