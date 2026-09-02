using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ApiAstilPos.Controllers
{ 
    [ApiController]
    [Route("api")]
    public class TiposDocumentoDianController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TiposDocumentoDianController> _logger;

        public TiposDocumentoDianController(IConfiguration configuration, ILogger<TiposDocumentoDianController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("tiposdocumentodian")]
        public async Task<IActionResult> GetTiposDocumentoDian()
        {
            _logger.LogInformation("Obteniendo lista de tipos de documento DIAN");

            try
            {
                var tiposDocumento = new List<TipoDocumentoDian>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposDocumentoDian", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonTiposDocumento = reader.IsDBNull(reader.GetOrdinal("tiposDocumentoDian"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("tiposDocumentoDian"));

                                tiposDocumento = JsonConvert.DeserializeObject<List<TipoDocumentoDian>>(jsonTiposDocumento) ?? new List<TipoDocumentoDian>();
                            }
                        }
                    }
                }

                return Ok(tiposDocumento);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tipos de documento DIAN: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}