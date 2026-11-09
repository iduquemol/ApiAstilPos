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
    public class TipoTrabajadorNominaDianController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TipoTrabajadorNominaDianController> _logger;

        public TipoTrabajadorNominaDianController(IConfiguration configuration, ILogger<TipoTrabajadorNominaDianController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString") ?? string.Empty;
        }

        [HttpGet("tipos-trabajador-nomina-dian")]
        public async Task<IActionResult> GetTiposTrabajadorNominaDian([FromQuery] long? idTipoTrabajadorDian)
        {
            _logger.LogInformation(idTipoTrabajadorDian.HasValue
                ? $"Obteniendo tipo de trabajador con ID: {idTipoTrabajadorDian.Value}"
                : "Obteniendo lista de tipos de trabajador nómina DIAN");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposTrabajadorNominaDianId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idTipoTrabajadorDian", (object)idTipoTrabajadorDian ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("tiposTrabajadorNominaDian");

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

                var lista = JsonConvert.DeserializeObject<List<TipoTrabajadorNominaDian>>(jsonCompleto) ?? new List<TipoTrabajadorNominaDian>();

                _logger.LogInformation($"Tipos de trabajador obtenidos: {lista.Count}");
                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tipos de trabajador nómina DIAN: {ex.Message}");
                return StatusCode(500, new { error = true, idTipoTrabajadorDian = idTipoTrabajadorDian ?? 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}