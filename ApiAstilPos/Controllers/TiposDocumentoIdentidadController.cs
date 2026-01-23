using Microsoft.AspNetCore.Mvc;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiposDocumentoIdentidadController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TiposDocumentoIdentidadController> _logger;

        public TiposDocumentoIdentidadController(IConfiguration configuration, ILogger<TiposDocumentoIdentidadController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetTiposDocumentoIdentidad()
        {
            _logger.LogInformation("Obteniendo lista de tipos de documento de identidad");

            try
            {
                var tipos = new List<TipoDocumentoIdentidad>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposDocumentoIdentidad", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tipos.Add(new TipoDocumentoIdentidad
                                {
                                    IdTipoDocumentoId = reader.GetInt16(reader.GetOrdinal("idTipoDocumentoId")),
                                    CodigoTipoDocumentoId = reader.IsDBNull(reader.GetOrdinal("codigoTipoDocumentoId"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("codigoTipoDocumentoId")),
                                    NombreTipoDocumentoId = reader.IsDBNull(reader.GetOrdinal("nombreTipoDocumentoId"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("nombreTipoDocumentoId"))
                                });
                            }
                        }
                    }
                }

                return Ok(tipos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tipos de documento de identidad: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}