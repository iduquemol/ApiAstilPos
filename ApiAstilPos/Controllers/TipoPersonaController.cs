using ApiAstilPos.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/tipospersona")]
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
                            if (await reader.ReadAsync())
                            {
                                int ordinal = reader.GetOrdinal("tiposPersona");
                                if (!reader.IsDBNull(ordinal))
                                {
                                    string jsonResult = reader.GetString(ordinal);

                                    // Opciones para ignorar mayúsculas/minúsculas al mapear propiedades JSON
                                    var options = new JsonSerializerOptions
                                    {
                                        PropertyNameCaseInsensitive = true
                                    };

                                    tipos = JsonSerializer.Deserialize<List<TipoPersona>>(jsonResult, options) ?? new List<TipoPersona>();
                                }
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