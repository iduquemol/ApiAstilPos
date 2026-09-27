using ApiAstilPos.Models;
using azureFunctionPos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
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

        [HttpGet("conceptosnotacredito")]
        public async Task<IActionResult> GetConceptosNotaCredito()
        {
            _logger.LogInformation("Obteniendo lista de conceptos de nota credito");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_conceptosNotaCredito", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("conceptoNotaCredito");

                            while (await reader.ReadAsync())
                            {
                                if (!reader.IsDBNull(ordinal))
                                {
                                    jsonBuilder.Append(reader.GetString(ordinal));
                                }
                            }
                        }
                    }
                }

                string jsonCompleto = jsonBuilder.ToString();

                if (string.IsNullOrWhiteSpace(jsonCompleto))
                {
                    jsonCompleto = "[]";
                }

                var conceptosNotaCredito = JsonConvert.DeserializeObject<List<ConceptoNotaCredito>>(jsonCompleto) ?? new List<ConceptoNotaCredito>();

                _logger.LogInformation($"Conceptos de Nota Credito obtenidos: {conceptosNotaCredito.Count}");
                return Ok(conceptosNotaCredito);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener Conceptos de Nota Credito: {ex.Message}");
                return StatusCode(500, new { error = true, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}