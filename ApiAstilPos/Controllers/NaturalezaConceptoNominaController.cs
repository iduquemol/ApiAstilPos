using ApiAstilPos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text;
using System.Text.Json;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class NaturalezaConceptoNominaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NaturalezaConceptoNominaController> _logger;

        public NaturalezaConceptoNominaController(IConfiguration configuration, ILogger<NaturalezaConceptoNominaController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString") ?? string.Empty;
        }

        [HttpGet("naturaleza-conceptos-nomina")]
        public async Task<IActionResult> GetNaturalezaConceptosNomina([FromQuery] long? idNaturalezaConcepto)
        {
            _logger.LogInformation(idNaturalezaConcepto.HasValue
                ? $"Obteniendo naturaleza de concepto con ID: {idNaturalezaConcepto.Value}"
                : "Obteniendo lista de naturaleza de conceptos de nómina");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tablanaturalezaConceptosNominaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idNaturalezaConcepto", (object)idNaturalezaConcepto ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("naturalezaConceptosNomina");

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

                var lista = JsonConvert.DeserializeObject<List<NaturalezaConceptoNomina>>(jsonCompleto) ?? new List<NaturalezaConceptoNomina>();

                _logger.LogInformation($"Naturalezas de conceptos obtenidas: {lista.Count}");
                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener naturaleza de conceptos de nómina: {ex.Message}");
                return StatusCode(500, new { error = true, idNaturalezaConcepto = idNaturalezaConcepto ?? 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}