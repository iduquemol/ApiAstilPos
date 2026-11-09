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
    public class ParametroNominaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ParametroNominaController> _logger;

        public ParametroNominaController(IConfiguration configuration, ILogger<ParametroNominaController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString") ?? string.Empty;
        }

        [HttpGet("parametros-nomina")]
        public async Task<IActionResult> GetParametrosNomina([FromQuery] long? idParametroNomina)
        {
            _logger.LogInformation(idParametroNomina.HasValue
                ? $"Obteniendo parámetro de nómina con ID: {idParametroNomina.Value}"
                : "Obteniendo lista de parámetros de nómina");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_parametrosNominaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idParametroNomina", (object)idParametroNomina ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("parametrosNomina");

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

                var lista = JsonConvert.DeserializeObject<List<ParametroNomina>>(jsonCompleto) ?? new List<ParametroNomina>();

                _logger.LogInformation($"Parámetros de nómina obtenidos: {lista.Count}");
                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener parámetros de nómina: {ex.Message}");
                return StatusCode(500, new { error = true, idParametroNomina = idParametroNomina ?? 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}