using ApiAstilPos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class ActividadesCiiuController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ActividadesCiiuController> _logger;

        public ActividadesCiiuController(IConfiguration configuration, ILogger<ActividadesCiiuController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("consultar-actividades-ciiu")]
        public async Task<IActionResult> GetActividadesCiiu([FromQuery] long? idActividadCiiu)
        {
            _logger.LogInformation(idActividadCiiu.HasValue
                ? $"Obteniendo actividad CIIU con ID: {idActividadCiiu.Value}"
                : "Obteniendo lista de actividades CIIU");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_actividadesCiiuId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@idActividadCiiu", (object)idActividadCiiu ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("actividadesCiiu");

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

                var actividades = JsonConvert.DeserializeObject<List<ActividadesCiiu>>(jsonCompleto) ?? new List<ActividadesCiiu>();

                _logger.LogInformation($"Actividades CIIU obtenidas: {actividades.Count}");
                return Ok(actividades);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener actividades CIIU: {ex.Message}");
                return StatusCode(500, new { error = true, idActividadCiiu = idActividadCiiu ?? 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}