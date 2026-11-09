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
    public class CotizacionController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CotizacionController> _logger;

        public CotizacionController(IConfiguration configuration, ILogger<CotizacionController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("consultar-cotizaciones")]
        public async Task<IActionResult> GetCotizaciones([FromQuery] long? idCotizacion)
        {
            _logger.LogInformation(idCotizacion.HasValue
                ? $"Obteniendo cotización con ID: {idCotizacion.Value}"
                : "Obteniendo lista de cotizaciones");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_cotizacionesId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@idCotizacion", (object)idCotizacion ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("cotizaciones");

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

                var cotizaciones = JsonConvert.DeserializeObject<List<Cotizacion>>(jsonCompleto) ?? new List<Cotizacion>();

                _logger.LogInformation($"Cotizaciones obtenidas: {cotizaciones.Count}");
                return Ok(cotizaciones);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener cotizaciones: {ex.Message}");
                return StatusCode(500, new { error = true, idCotizacion = idCotizacion ?? 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("crear-cotizaciones")]
        public async Task<IActionResult> CreateCotizacion([FromBody] Cotizacion cotizacion)
        {
            _logger.LogInformation("Creando una nueva cotizacion");

            try
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                };

                string requestBody = JsonConvert.SerializeObject(cotizacion, settings);
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                object cotizacionId = null;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_cotizacion", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@cotizacion", requestBody ?? (object)DBNull.Value);

                        cotizacionId = await command.ExecuteScalarAsync();
                    }

                    if (cotizacionId == null || cotizacionId == DBNull.Value || Convert.ToInt64(cotizacionId) <= 0)
                    {
                        _logger.LogError("No se pudo obtener un ID de cotizacion válido. El SP devolvió NULL.");
                        return BadRequest("Error: No se pudo crear la cotización en la base de datos.");
                    }
                    else
                    {
                        long idValido = Convert.ToInt64(cotizacionId);
                        _logger.LogInformation($"Cotizacion creada correctamente con ID: {idValido}");
                        return Ok(new { message = "Cotizacion creada correctamente", idCotizacion = idValido });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear cotizacion: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("obtener-cotizacion")]
        public async Task<IActionResult> PostCotizacionId([FromBody] JsonElement request)
        {
            _logger.LogInformation("Procesando solicitud para obtener cotización por ID");

            try
            {
                long idCotizacion = 0;

                foreach (var property in request.EnumerateObject())
                {
                    if (string.Equals(property.Name, "idCotizacion", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(property.Name, "idcotizacion", StringComparison.OrdinalIgnoreCase))
                    {
                        if (property.Value.TryGetInt64(out long val))
                        {
                            idCotizacion = val;
                            break;
                        }
                        else if (property.Value.TryGetInt32(out int valInt))
                        {
                            idCotizacion = valInt;
                            break;
                        }
                    }
                }

                if (idCotizacion == 0)
                {
                    _logger.LogWarning($"El parámetro idcotizacion no se pudo extraer del JSON recibido: {request.GetRawText()}");
                    return BadRequest("El parámetro idcotizacion es requerido o inválido.");
                }

                Cotizacion cotizacion = null;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_cotizacionesId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idCotizacion", idCotizacion);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonCotizacion = reader.IsDBNull(reader.GetOrdinal("cotizaciones"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("cotizaciones"));

                                if (!string.IsNullOrEmpty(jsonCotizacion))
                                {
                                    if (jsonCotizacion.TrimStart().StartsWith("["))
                                    {
                                        var listaCotizaciones = JsonConvert.DeserializeObject<List<Cotizacion>>(jsonCotizacion);
                                        cotizacion = listaCotizaciones?.FirstOrDefault();
                                    }
                                    else
                                    {
                                        cotizacion = JsonConvert.DeserializeObject<Cotizacion>(jsonCotizacion);
                                    }
                                }
                            }
                        }
                    }
                }

                if (cotizacion == null)
                {
                    return NotFound("No se encontró la cotización especificada.");
                }

                _logger.LogInformation("Cotización obtenida correctamente.");
                return Ok(cotizacion);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener cotización: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("actualizar-cotizacion")]
        public async Task<IActionResult> UpdateCotizacion([FromBody] Cotizacion cotizacion)
        {
            _logger.LogInformation("Actualizando una cotización");

            try
            {
                if (cotizacion.IdCotizacion <= 0)
                {
                    return BadRequest("El idCotizacion es requerido para actualizar.");
                }

                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                };

                string requestBody = JsonConvert.SerializeObject(cotizacion, settings);
                _logger.LogInformation($"Cuerpo de la solicitud de actualización: {requestBody}");

                object cotizacionId = null;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_cotizacion", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@cotizacion", requestBody ?? (object)DBNull.Value);

                        cotizacionId = await command.ExecuteScalarAsync();
                    }

                    if (cotizacionId == null || cotizacionId == DBNull.Value || Convert.ToInt64(cotizacionId) <= 0)
                    {
                        _logger.LogError("No se pudo actualizar la cotización en la base de datos.");
                        return BadRequest("Error: No se pudo actualizar la cotización.");
                    }
                    else
                    {
                        long idValido = Convert.ToInt64(cotizacionId);
                        _logger.LogInformation($"Cotización actualizada correctamente con ID: {idValido}");
                        return Ok(new { message = "Cotización actualizada correctamente", idCotizacion = idValido });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar cotización: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}