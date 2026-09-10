using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        public async Task<IActionResult> GetTiposDocumentoIdentidad([FromQuery] short? idTipoDocumentoId = null)
        {
            _logger.LogInformation("Obteniendo lista de tipos de documento de identidad");

            try
            {
                var tipos = new List<TipoDocumentoIdentidad>();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposDocumentoIdentidadId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetro opcional para filtrar por ID
                        command.Parameters.AddWithValue("@idTipoDocumentoId", (object)idTipoDocumentoId ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonTipos = reader.IsDBNull(reader.GetOrdinal("tiposDocumentoIdentidad"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("tiposDocumentoIdentidad"));

                                var listaTemporal = JsonConvert.DeserializeObject<List<TipoDocumentoIdentidad>>(jsonTipos);
                                if (listaTemporal != null)
                                {
                                    tipos.AddRange(listaTemporal);
                                }
                            }
                        }
                    }
                }

                _logger.LogInformation($"Tipos de documento obtenidos: {tipos.Count}");
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