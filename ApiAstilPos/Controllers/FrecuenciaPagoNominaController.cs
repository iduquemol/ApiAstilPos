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
    public class FrecuenciaPagoNominaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FrecuenciaPagoNominaController> _logger;

        public FrecuenciaPagoNominaController(IConfiguration configuration, ILogger<FrecuenciaPagoNominaController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString") ?? string.Empty;
        }

        [HttpGet("frecuencia-pagos-nomina")]
        public async Task<IActionResult> GetFrecuenciaPagosNomina([FromQuery] long? idFrecuenciaPago)
        {
            _logger.LogInformation(idFrecuenciaPago.HasValue
                ? $"Obteniendo frecuencia de pago con ID: {idFrecuenciaPago.Value}"
                : "Obteniendo lista de frecuencia de pagos de nómina");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tablafrecuenciaPagosNominaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idFrecuenciaPago", (object)idFrecuenciaPago ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("frecuenciaPagosNomina");

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

                var lista = JsonConvert.DeserializeObject<List<FrecuenciaPagoNomina>>(jsonCompleto) ?? new List<FrecuenciaPagoNomina>();

                _logger.LogInformation($"Frecuencias de pago obtenidas: {lista.Count}");
                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener frecuencia de pagos de nómina: {ex.Message}");
                return StatusCode(500, new { error = true, idFrecuenciaPago = idFrecuenciaPago ?? 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}