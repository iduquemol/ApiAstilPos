using Microsoft.AspNetCore.Mvc;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/unidadesdemedida")]
    public class UnidadDeMedidaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UnidadDeMedidaController> _logger;

        public UnidadDeMedidaController(IConfiguration configuration, ILogger<UnidadDeMedidaController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetUnidadesDeMedida()
        {
            _logger.LogInformation("Obteniendo lista de unidades de medida");

            try
            {
                var unidades = new List<UnidadDeMedida>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_unidadesMedida", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                unidades.Add(new UnidadDeMedida
                                {
                                    IdUnidadMedida = reader.GetInt64(reader.GetOrdinal("idUnidadMedida")),
                                    CodigoUnidadMedida = reader.IsDBNull(reader.GetOrdinal("codigoUnidadMedida"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("codigoUnidadMedida")),
                                    NombreUnidadMedida = reader.IsDBNull(reader.GetOrdinal("nombreUnidadMedida"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("nombreUnidadMedida")),
                                });
                            }
                        }
                    }
                }

                return Ok(unidades);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener unidades de medida: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}