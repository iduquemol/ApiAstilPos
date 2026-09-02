using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class RolesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IConfiguration configuration, ILogger<RolesController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            _logger.LogInformation("Obteniendo lista de roles");

            try
            {
                var roles = new List<Rol>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_roles", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonRoles = reader.IsDBNull(reader.GetOrdinal("roles"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("roles"));

                                roles = JsonConvert.DeserializeObject<List<Rol>>(jsonRoles) ?? new List<Rol>();
                            }
                        }
                    }
                }

                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener roles: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}