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

        [HttpGet("tiposDocumento")]
        public async Task<IActionResult> GetTiposDocumento()
        {
            _logger.LogInformation("Obteniendo lista de tipos de documento");

            try
            {
                var tiposDocumento = new List<TipoDocumento>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposDocumentoId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var ordinal = reader.GetOrdinal("tiposDocumento");
                                var jsonTiposDocumento = reader.IsDBNull(ordinal)
                                    ? "[]"
                                    : reader.GetString(ordinal);

                                tiposDocumento = JsonConvert.DeserializeObject<List<TipoDocumento>>(jsonTiposDocumento)
                                                 ?? new List<TipoDocumento>();
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
        public async Task<IActionResult> GetTiposDocumentoNotaCredito([FromQuery] long? idTipoDocumentoExterno = null)
        {
            _logger.LogInformation("Obteniendo lista de tipos de documento de notas crédito");

            try
            {
                string jsonTiposDocumento = "[]";

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposDocumentoNotasCredito", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@idTipoDocumentoExterno", SqlDbType.BigInt).Value =
                            (object)idTipoDocumentoExterno ?? DBNull.Value;

                        var result = await command.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            jsonTiposDocumento = result.ToString();
                        }
                    }
                }

                var tiposDocumento = JsonConvert.DeserializeObject<List<TipoDocumento>>(jsonTiposDocumento)
                                     ?? new List<TipoDocumento>();

                _logger.LogInformation($"Tipos de documento de nota crédito obtenidos: {tiposDocumento.Count}");
                return Ok(tiposDocumento);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tipos de documento de nota crédito: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("tipos-documento-dsa")]
        public async Task<IActionResult> GetTiposDocumentoDsa([FromQuery] long? idTipoDocumentoExterno = null)
        {
            _logger.LogInformation("Obteniendo tipos de documento DSA");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_Read_tiposDocumentoDsa", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("@idTipoDocumentoExterno", SqlDbType.BigInt).Value =
                            (object)idTipoDocumentoExterno ?? DBNull.Value;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("tiposDocumentoDSA");

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

                string jsonResult = jsonBuilder.ToString();

                if (string.IsNullOrWhiteSpace(jsonResult))
                {
                    return Ok(new List<TipoDocumento>());
                }

                var tiposDocumento = JsonConvert.DeserializeObject<List<TipoDocumento>>(jsonResult);

                _logger.LogInformation($"Tipos de documento DSA obtenidos: {tiposDocumento?.Count ?? 0}");
                return Ok(tiposDocumento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de documento DSA");
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error interno del servidor", detalle = ex.Message });
            }
        }
    }
}