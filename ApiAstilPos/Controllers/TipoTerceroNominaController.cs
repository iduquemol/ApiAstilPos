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
    public class TipoTerceroNominaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TipoTerceroNominaController> _logger;

        public TipoTerceroNominaController(IConfiguration configuration, ILogger<TipoTerceroNominaController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString") ?? string.Empty;
        }

        [HttpGet("tipos-tercero-nomina")]
        public async Task<IActionResult> GetTiposTerceroNomina([FromQuery] long? idTipoTercero)
        {
            _logger.LogInformation(idTipoTercero.HasValue
                ? $"Obteniendo tipo de tercero con ID: {idTipoTercero.Value}"
                : "Obteniendo lista de tipos de tercero nómina");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposTerceroNominaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idTipoTercero", (object)idTipoTercero ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("tiposTerceroNomina");

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

                var lista = JsonConvert.DeserializeObject<List<TipoTerceroNomina>>(jsonCompleto) ?? new List<TipoTerceroNomina>();

                _logger.LogInformation($"Tipos de tercero obtenidos: {lista.Count}");
                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tipos de tercero nómina: {ex.Message}");
                return StatusCode(500, new { error = true, idTipoTercero = idTipoTercero ?? 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}