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
    public class TipoContratoNominaDianController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TipoContratoNominaDianController> _logger;

        public TipoContratoNominaDianController(IConfiguration configuration, ILogger<TipoContratoNominaDianController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString") ?? string.Empty;
        }

        [HttpGet("tipos-contrato-nomina-dian")]
        public async Task<IActionResult> GetTiposContratoNominaDian([FromQuery] long? idTipoContratoDian)
        {
            _logger.LogInformation(idTipoContratoDian.HasValue
                ? $"Obteniendo tipo de contrato con ID: {idTipoContratoDian.Value}"
                : "Obteniendo lista de tipos de contrato nómina DIAN");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tablatiposContratoNominaDianId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idTipoContratoDian", (object)idTipoContratoDian ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("tiposContratoNominaDian");

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

                var lista = JsonConvert.DeserializeObject<List<TipoContratoNominaDian>>(jsonCompleto) ?? new List<TipoContratoNominaDian>();

                _logger.LogInformation($"Tipos de contrato obtenidos: {lista.Count}");
                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tipos de contrato nómina DIAN: {ex.Message}");
                return StatusCode(500, new { error = true, idTipoContratoDian = idTipoContratoDian ?? 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}