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
    public class ConceptoNotaCreditoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConceptoNotaCreditoController> _logger;

        public ConceptoNotaCreditoController(IConfiguration configuration, ILogger<ConceptoNotaCreditoController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetConceptosNotaCredito()
        {
            _logger.LogInformation("Obteniendo lista de conceptos de nota credito");

            try
            {
                var conceptosNotaCredito = new List<ConceptoNotaCredito>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_conceptosNotaCredito", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonConceptosNotaCredito = reader.IsDBNull(reader.GetOrdinal("conceptoNotaCredito"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("conceptoNotaCredito"));
                                conceptosNotaCredito = JsonConvert.DeserializeObject<List<ConceptoNotaCredito>>(jsonConceptosNotaCredito);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Conceptos de Nota Credito obtenidos: {conceptosNotaCredito.Count}");
                return Ok(conceptosNotaCredito);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener Conceptos de Nota Credito: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}