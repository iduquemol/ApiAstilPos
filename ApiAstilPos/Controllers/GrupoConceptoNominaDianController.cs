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
    public class GrupoConceptoNominaDianController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GrupoConceptoNominaDianController> _logger;

        public GrupoConceptoNominaDianController(IConfiguration configuration, ILogger<GrupoConceptoNominaDianController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString") ?? string.Empty;
        }

        [HttpGet("grupos-concepto-nomina-dian")]
        public async Task<IActionResult> GetGruposConceptoNominaDian([FromQuery] long? idGrupoConcepto)
        {
            _logger.LogInformation(idGrupoConcepto.HasValue
                ? $"Obteniendo grupo de concepto con ID: {idGrupoConcepto.Value}"
                : "Obteniendo lista de grupos de concepto de nómina DIAN");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tablagruposConceptoNominaDianId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idGrupoConcepto", (object)idGrupoConcepto ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("gruposConceptoNominaDian");

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

                var lista = JsonConvert.DeserializeObject<List<GrupoConceptoNominaDian>>(jsonCompleto) ?? new List<GrupoConceptoNominaDian>();

                _logger.LogInformation($"Grupos de concepto obtenidos: {lista.Count}");
                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener grupos de concepto de nómina DIAN: {ex.Message}");
                return StatusCode(500, new { error = true, idGrupoConcepto = idGrupoConcepto ?? 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}