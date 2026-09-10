using ApiAstilPos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class ResolucionesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ResolucionesController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ResolucionesController(IConfiguration configuration, ILogger<ResolucionesController> logger, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("resoluciones")]
        public async Task<IActionResult> GetResoluciones()
        {
            try
            {
                string jsonResult = string.Empty;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_resolucionesFacturacionId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var ordinal = reader.GetOrdinal("resolucionesFacturacion");
                                if (!reader.IsDBNull(ordinal))
                                {
                                    jsonResult = reader.GetString(ordinal);
                                }
                            }
                        }
                    }
                }

                // Si la tabla está vacía o el SP devuelve nulo/vacío
                if (string.IsNullOrWhiteSpace(jsonResult) || jsonResult.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    return Ok(new List<resoluciones>());
                }

                var resoluciones = JsonConvert.DeserializeObject<List<resoluciones>>(jsonResult)
                                   ?? new List<resoluciones>();

                return Ok(resoluciones);
            }
            catch (Exception ex)
            {
                // Registrar la excepción exacta para depuración
                _logger.LogError(ex, "Error al obtener resoluciones: {Message}", ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("sincronizar-resoluciones-externas")]
        public async Task<IActionResult> SincronizarResolucionesExternas()
        {
            _logger.LogInformation("Iniciando sincronización de resoluciones desde el proveedor externo");

            try
            {
                var resolutionsUrl = _configuration["ApiExterna:ResolutionsUrl"];
                var bearerToken = _configuration["ApiExterna:BearerToken"];

                if (string.IsNullOrEmpty(resolutionsUrl))
                {
                    _logger.LogError("URL de consulta externa de resoluciones no configurada");
                    return BadRequest("Configuración de API externa faltante");
                }

                using var httpClient = _httpClientFactory?.CreateClient() ?? new HttpClient();

                httpClient.DefaultRequestHeaders.Clear();
                if (!string.IsNullOrEmpty(bearerToken))
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
                }

                _logger.LogInformation($"Consultando proveedor externo en: {resolutionsUrl}");
                var response = await httpClient.GetAsync(resolutionsUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error en API externa de resoluciones: {response.StatusCode} - {responseContent}");
                    return BadRequest("Error al consultar las resoluciones en el proveedor externo");
                }

                // Ejecución del Stored Procedure sp_Update_resolucionesFacturacionDian con parámetros OUTPUT
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_resolucionesFacturacionDian", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetro de entrada con el JSON de la DIAN
                        command.Parameters.AddWithValue("@resolucionesDian", responseContent ?? (object)DBNull.Value);

                        // Parámetros de salida indicados por la base de datos
                        var errorParam = new SqlParameter("@errorOutput", SqlDbType.Bit)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var mensajeParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) // -1 equivale a max
                        {
                            Direction = ParameterDirection.Output
                        };

                        command.Parameters.Add(errorParam);
                        command.Parameters.Add(mensajeParam);

                        await command.ExecuteNonQueryAsync();

                        // Lectura de los valores que devuelve la BD
                        bool isError = errorParam.Value != DBNull.Value && Convert.ToBoolean(errorParam.Value);
                        string mensajeBd = mensajeParam.Value != DBNull.Value ? mensajeParam.Value.ToString() : string.Empty;

                        if (isError)
                        {
                            _logger.LogError($"Error en el procedimiento almacenado: {mensajeBd}");
                            return BadRequest($"Error en base de datos: {mensajeBd}");
                        }

                        _logger.LogInformation($"SP ejecutado correctamente. Mensaje BD: {mensajeBd}");
                    }
                }

                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    return Ok(new object[] { });
                }

                // Deserializar el JSON obtenido para devolver la estructura real al frontend
                var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<object>(responseContent);

                return Ok(jsonResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al sincronizar resoluciones externas: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("resoluciones")]
        public async Task<IActionResult> Createresoluciones([FromBody] JsonElement resolucionesJson)
        {
            _logger.LogInformation("Creando una nueva resolucion");

            try
            {
                string requestBody = resolucionesJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_resolucionesFacturacion", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@resolucionesFacturacion", requestBody ?? (object)DBNull.Value);

                        // Ejecutar el SP
                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Resolución creada correctamente.");
                        return Ok(new { message = "Resolución creada correctamente.", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear resolución: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPut("resoluciones")]
        public async Task<IActionResult> Updateresoluciones([FromBody] JsonElement resolucionesJson)
        {
            _logger.LogInformation("Actualizando una resolución");

            try
            {
                string requestBody = resolucionesJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_resolucionesFacturacionDian", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@resolucionesFacturacion", requestBody ?? (object)DBNull.Value);

                        // Ejecutar el SP
                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Resolución actualizada correctamente.");
                        return Ok(new { message = "Resolución actualizada correctamente.", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar resolución: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete("resoluciones")]
        public async Task<IActionResult> Deleteresoluciones([FromBody] JObject request)
        {
            _logger.LogInformation("Borrando resolución");

            try
            {
                var idResoluciones = request["idResoluciones"]?.Value<long>() ?? 0;
                _logger.LogInformation($"ID resolución a borrar: {idResoluciones}");
                var idresolucionesBorrado = 0;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_resolucionesFacturacion", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idResoluciones", idResoluciones);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                idresolucionesBorrado = reader.IsDBNull(reader.GetOrdinal("idResoluciones"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("idResoluciones"));
                            }
                        }

                        _logger.LogInformation("Resolución borrada correctamente.");
                        return Ok(new { message = "Resolución borrada correctamente.", idresolucionesBorrado = idresolucionesBorrado });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al borrar resolución: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}