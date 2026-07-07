using ApiAstilPos.Models;
using ApiAstilPos.Services;
using azureFunctionPos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/portal")]
    public class PortalController : ControllerBase
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PortalController> _logger;

        public PortalController(EmailService emailService, IConfiguration configuration, ILogger<PortalController> logger)
        {
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpPost("consulta-facturas")]
        public async Task<IActionResult> ConsultaFacturas([FromBody] JsonElement request)
        {
            _logger.LogInformation("Procesando solicitud para obtener facturas");

            try
            {
                var fechaInicial = request.TryGetProperty("fechaInicial", out var fechaInicialElement)
                    ? fechaInicialElement.GetDateTime()
                    : DateTime.MinValue;

                var fechaFinal = request.TryGetProperty("fechaFinal", out var fechaFinalElement)
                    ? fechaFinalElement.GetDateTime()
                    : DateTime.MaxValue;

                var idMetodoDian = request.TryGetProperty("idMetodoDian", out var idMetodoDianElement)
                    ? idMetodoDianElement.GetInt32()
                    : 0;

                var prefijo = request.TryGetProperty("prefijo", out var prefijoElement)
                    ? prefijoElement.GetString()
                    : string.Empty;

                var respuestaConsulta = new List<RespuestaConsultaFactura>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_documentosDian", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@fechaInicial", fechaInicial);
                        command.Parameters.AddWithValue("@fechaFinal", fechaFinal);
                        command.Parameters.AddWithValue("@idMetodoDian", idMetodoDian);
                        command.Parameters.AddWithValue("@prefijo", prefijo);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonRespuesta = reader.IsDBNull(reader.GetOrdinal("documentos")) ? "[]" : reader.GetString(reader.GetOrdinal("documentos"));
                                respuestaConsulta = JsonConvert.DeserializeObject<List<RespuestaConsultaFactura>>(jsonRespuesta);                                
                            }
                        }

                        _logger.LogInformation("Consulta facturas obtenida correctamente.");
                        return Ok(respuestaConsulta);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener consulta facturas: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("consulta-prefijos")]
        public async Task<IActionResult> ObtenerPrefijos([FromBody] JsonElement request)
        {
            _logger.LogInformation("Obteniendo lista de prefijos");

            try
            {
                var idMetodoDian = request.TryGetProperty("idMetodoDian", out var idMetodoDianElement)
                   ? idMetodoDianElement.GetInt32()
                   : 0;

                var prefijosLista = new List<RespuestaPrefijo>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_Prefijos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idMetodoDian", idMetodoDian);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonPrefijosLista = reader.IsDBNull(reader.GetOrdinal("prefijos"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("prefijos"));
                                prefijosLista = JsonConvert.DeserializeObject<List<RespuestaPrefijo>>(jsonPrefijosLista);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Prefijos obtenidos: {prefijosLista.Count}");
                return Ok(prefijosLista);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener prefijos: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
