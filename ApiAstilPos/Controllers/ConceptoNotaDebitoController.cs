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
    public class ConceptoNotaDebitoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConceptoNotaDebitoController> _logger;

        public ConceptoNotaDebitoController(IConfiguration configuration, ILogger<ConceptoNotaDebitoController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("conceptosnotadebito")]
        public async Task<IActionResult> GetConceptosNotaDebito()
        {
            _logger.LogInformation("Obteniendo lista de conceptos de nota débito");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_conceptosNotaDebito", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("conceptoNotaDebito");

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

                var conceptosNotaDebito = JsonConvert.DeserializeObject<List<ConceptoNotaDebito>>(jsonCompleto) ?? new List<ConceptoNotaDebito>();

                _logger.LogInformation($"Conceptos de Nota Credito obtenidos: {conceptosNotaDebito.Count}");
                return Ok(conceptosNotaDebito);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener Conceptos de Nota Débito: {ex.Message}");
                return StatusCode(500, new { error = true, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}