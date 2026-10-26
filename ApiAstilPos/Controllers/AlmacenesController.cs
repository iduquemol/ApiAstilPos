using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Text;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class AlmacenesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AlmacenesController> _logger;

        public AlmacenesController(IConfiguration configuration, ILogger<AlmacenesController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString") ?? string.Empty;
        }

        /// <summary>
        /// Método auxiliar para extraer el mensaje formateado desde el output del Stored Procedure.
        /// </summary>
        private string ExtraerMensajeDb(string? mensajeRaw, string mensajePorDefecto)
        {
            if (string.IsNullOrWhiteSpace(mensajeRaw))
                return mensajePorDefecto;

            try
            {
                var listaMensajes = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(mensajeRaw);
                if (listaMensajes != null && listaMensajes.Count > 0 && listaMensajes[0].ContainsKey("mensaje"))
                {
                    return listaMensajes[0]["mensaje"]?.ToString() ?? mensajePorDefecto;
                }
            }
            catch
            {
                return mensajeRaw;
            }

            return mensajePorDefecto;
        }

        [HttpGet("almacenes")]
        public async Task<IActionResult> GetAlmacenes([FromQuery] long? idAlmacen = null)
        {
            _logger.LogInformation("Obteniendo lista de almacenes");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_almacenesId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idAlmacen", (object?)idAlmacen ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("almacenes");

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

                var almacenes = JsonConvert.DeserializeObject<List<Almacenes>>(jsonCompleto) ?? new List<Almacenes>();

                _logger.LogInformation($"Almacenes obtenidos: {almacenes.Count}");
                return Ok(almacenes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener almacenes: {ex.Message}");
                return StatusCode(500, new { error = true, idAlmacen = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("almacenes")]
        public async Task<IActionResult> CreateAlmacen([FromBody] JsonElement almacenesJson)
        {
            _logger.LogInformation("Creando un nuevo almacén");

            try
            {
                string requestBody = almacenesJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_almacenes", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@almacenes", requestBody ?? (object)DBNull.Value);

                        var idAlmacenParam = new SqlParameter("@idAlmacen", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idAlmacenParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idAlmacenParam.Value != DBNull.Value ? Convert.ToInt64(idAlmacenParam.Value) : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Almacén creado correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al crear almacén: {mensajeTexto}");
                            return BadRequest(new { error = true, idAlmacen = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Almacén creado con ID: {idRetornado}");
                        return Ok(new { error = false, idAlmacen = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear almacén: {ex.Message}");
                return StatusCode(500, new { error = true, idAlmacen = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPut("almacenes")]
        public async Task<IActionResult> UpdateAlmacen([FromBody] JsonElement almacenesJson)
        {
            _logger.LogInformation("Actualizando un almacén");

            try
            {
                string requestBody = almacenesJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_almacenes", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@almacenes", requestBody ?? (object)DBNull.Value);

                        var idAlmacenParam = new SqlParameter("@idAlmacen", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idAlmacenParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idAlmacenParam.Value != DBNull.Value ? Convert.ToInt64(idAlmacenParam.Value) : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Almacén actualizado correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al actualizar almacén: {mensajeTexto}");
                            return BadRequest(new { error = true, idAlmacen = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Almacén con ID {idRetornado} actualizado correctamente.");
                        return Ok(new { error = false, idAlmacen = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar almacén: {ex.Message}");
                return StatusCode(500, new { error = true, idAlmacen = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpDelete("almacenes/{id}")]
        public async Task<IActionResult> DeleteAlmacen(long id)
        {
            _logger.LogInformation($"Intentando eliminar el almacén con ID: {id}");

            try
            {
                string requestBody = JsonConvert.SerializeObject(new { idAlmacen = id });

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_almacenes", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@almacenes", requestBody);

                        var idAlmacenParam = new SqlParameter("@idAlmacen", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idAlmacenParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idAlmacenParam.Value != DBNull.Value ? Convert.ToInt64(idAlmacenParam.Value) : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Almacén eliminado correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al eliminar almacén: {mensajeTexto}");
                            return BadRequest(new { error = true, idAlmacen = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Almacén con ID {idRetornado} eliminado exitosamente.");
                        return Ok(new { error = false, idAlmacen = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar almacén: {ex.Message}");
                return StatusCode(500, new { error = true, idAlmacen = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}