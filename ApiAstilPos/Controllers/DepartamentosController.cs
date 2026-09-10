using ApiAstilPos.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/departamentos")]
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
        public async Task<IActionResult> GetDepartamentos([FromQuery] long? idDepartamento = null)
        {
            _logger.LogInformation("Obteniendo lista de departamentos");

            try
            {
                var departamentos = new List<Departamento>();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_departamentosId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@idDepartamento", (object)idDepartamento ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                int ordinal = reader.GetOrdinal("departamentos");
                                if (!reader.IsDBNull(ordinal))
                                {
                                    string jsonResult = reader.GetString(ordinal);

                                    var options = new JsonSerializerOptions
                                    {
                                        PropertyNameCaseInsensitive = true
                                    };

                                    departamentos = JsonSerializer.Deserialize<List<Departamento>>(jsonResult, options) ?? new List<Departamento>();
                                }
                            }
                        }
                    }
                }

                return Ok(departamentos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener los departamentos: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}