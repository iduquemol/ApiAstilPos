using ApiAstilPos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class ConceptosRechazoDianController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConceptosRechazoDianController> _logger;

        public ConceptosRechazoDianController(IConfiguration configuration, ILogger<ConceptosRechazoDianController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("conceptos-rechazo-dian")]
        public async Task<IActionResult> GetConceptosRechazoDian([FromQuery] long? idConceptoRechazoDian = null)
        {
            _logger.LogInformation("Obteniendo lista de conceptos de rechazo DIAN");

            try
            {
                var conceptosRechazoDian = new List<ConceptosRechazoDian>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_conceptosRechazoDianId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idConceptoRechazoDian", (object?)idConceptoRechazoDian ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var json = reader.IsDBNull(reader.GetOrdinal("conceptosRechazoDian"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("conceptosRechazoDian"));

                                conceptosRechazoDian = JsonConvert.DeserializeObject<List<ConceptosRechazoDian>>(json) ?? new List<ConceptosRechazoDian>();
                            }
                        }
                    }
                }

                _logger.LogInformation($"Conceptos de rechazo DIAN obtenidos: {conceptosRechazoDian.Count}");
                return Ok(conceptosRechazoDian );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener conceptos de rechazo DIAN");
                return StatusCode(500, new { error = true, idConceptoRechazoDian = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}