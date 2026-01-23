using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class TipoDocumentoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TipoDocumentoController> _logger;

        public TipoDocumentoController(IConfiguration configuration, ILogger<TipoDocumentoController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetTiposDocumento()
        {
            _logger.LogInformation("Obteniendo lista de tipos de documento");

            try
            {
                var tiposDocumento = new List<TipoDocumento>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposdocumento", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonTiposDocumento = reader.IsDBNull(reader.GetOrdinal("tiposDocumento"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("tiposDocumento"));
                                tiposDocumento = JsonConvert.DeserializeObject<List<TipoDocumento>>(jsonTiposDocumento);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Tipos de documento obtenidos: {tiposDocumento.Count}");
                return Ok(tiposDocumento);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tipos de documento: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("tiposDocumentoVenta")]
        public async Task<IActionResult> GetTiposDocumentoVenta()
        {
            _logger.LogInformation("Obteniendo lista de tipos de documento de venta");

            try
            {
                var tiposDocumento = new List<TipoDocumento>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposDocumentoVentas", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonTiposDocumento = reader.IsDBNull(reader.GetOrdinal("tiposDocumentoVentas"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("tiposDocumentoVentas"));
                                tiposDocumento = JsonConvert.DeserializeObject<List<TipoDocumento>>(jsonTiposDocumento);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Tipos de documento de venta obtenidos: {tiposDocumento.Count}");
                return Ok(tiposDocumento);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tipos de documento de venta: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("notacredito")]
        public async Task<IActionResult> GetTiposDocumentoNotaCredito()
        {
            _logger.LogInformation("Obteniendo lista de tipos de documento de notas credito");

            try
            {
                var tiposDocumento = new List<TipoDocumento>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposDocumentoNotasCredito", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonTiposDocumento = reader.IsDBNull(reader.GetOrdinal("tiposDocumentoNotasCredito"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("tiposDocumentoNotasCredito"));
                                tiposDocumento = JsonConvert.DeserializeObject<List<TipoDocumento>>(jsonTiposDocumento);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Tipos de documento de nota credito obtenidos: {tiposDocumento.Count}");
                return Ok(tiposDocumento);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tipos de documento de nota credito: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}