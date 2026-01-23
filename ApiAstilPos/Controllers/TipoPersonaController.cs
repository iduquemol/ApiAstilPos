using azureFunctionPos.Models;
using Microsoft.AspNetCore.Mvc;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoPersonaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TipoPersonaController> _logger;

        public TipoPersonaController(IConfiguration configuration, ILogger<TipoPersonaController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetTiposPersona()
        {
            _logger.LogInformation("Obteniendo lista de tipos de persona");

            try
            {
                var tipos = new List<TipoPersona>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposPersona", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tipos.Add(new TipoPersona
                                {
                                    IdTipoPersona = reader.GetInt16(reader.GetOrdinal("idTipoPersona")),
                                    CodigoTipoPersona = reader.IsDBNull(reader.GetOrdinal("codigoTipoPersona"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("codigoTipoPersona")),
                                    NombreTipoPersona = reader.IsDBNull(reader.GetOrdinal("nombreTipoPersona"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("nombreTipoPersona"))
                                });
                            }
                        }
                    }
                }

                return Ok(tipos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tipos de persona: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}