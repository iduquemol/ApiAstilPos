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
    public class SucursalesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SucursalesController> _logger;

        public SucursalesController(IConfiguration configuration, ILogger<SucursalesController> logger)
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
        private string ExtraerMensajeDb(string mensajeRaw, string mensajePorDefecto)
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

        [HttpGet("sucursales")]
        public async Task<IActionResult> GetSucursales()
        {
            _logger.LogInformation("Obteniendo lista de sucursales");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_sucursalesId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("sucursales");

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

                var sucursales = JsonConvert.DeserializeObject<List<Sucursal>>(jsonCompleto) ?? new List<Sucursal>();

                _logger.LogInformation($"Sucursales obtenidas: {sucursales.Count}");
                return Ok(sucursales);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener sucursales: {ex.Message}");
                return StatusCode(500, new { error = true, idSucursal = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("sucursales")]
        public async Task<IActionResult> CreateSucursal([FromBody] JsonElement sucursalesJson)
        {
            _logger.LogInformation("Creando una nueva sucursal");

            try
            {
                string requestBody = sucursalesJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_sucursales", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@sucursales", requestBody ?? (object)DBNull.Value);

                        var idSucursalParam = new SqlParameter("@idSucursal", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idSucursalParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idSucursalParam.Value != DBNull.Value ? Convert.ToInt64(idSucursalParam.Value) : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Sucursal creada correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al crear sucursal: {mensajeTexto}");
                            return BadRequest(new { error = true, idSucursal = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Sucursal creada con ID: {idRetornado}");
                        return Ok(new { error = false, idSucursal = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear sucursal: {ex.Message}");
                return StatusCode(500, new { error = true, idSucursal = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPut("sucursales")]
        public async Task<IActionResult> UpdateSucursal([FromBody] JsonElement sucursalesJson)
        {
            _logger.LogInformation("Actualizando una sucursal");

            try
            {
                string requestBody = sucursalesJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_sucursales", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@sucursales", requestBody ?? (object)DBNull.Value);

                        var idSucursalParam = new SqlParameter("@idSucursal", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idSucursalParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idSucursalParam.Value != DBNull.Value ? Convert.ToInt64(idSucursalParam.Value) : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Sucursal actualizada correctamente");

                        if (tieneError)
                        {
                            _logger.LogWarning($"Error al actualizar sucursal: {mensajeTexto}");
                            return BadRequest(new { error = true, idSucursal = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Sucursal con ID {idRetornado} actualizada correctamente.");
                        return Ok(new { error = false, idSucursal = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar sucursal: {ex.Message}");
                return StatusCode(500, new { error = true, idSucursal = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpDelete("sucursales/{id}")]
        public async Task<IActionResult> DeleteSucursal(long id)
        {
            _logger.LogInformation($"Intentando eliminar la sucursal con ID: {id}");

            try
            {
                // Construimos el JSON que espera el SP sp_Delete_sucursales
                string requestBody = JsonConvert.SerializeObject(new { idSucursal = id });

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_sucursales", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@sucursales", requestBody);

                        var idSucursalParam = new SqlParameter("@idSucursal", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idSucursalParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idSucursalParam.Value != DBNull.Value ? Convert.ToInt64(idSucursalParam.Value) : id;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Sucursal eliminada correctamente");

                        if (tieneError)
                        {
                            _logger.LogWarning($"Error al eliminar sucursal: {mensajeTexto}");
                            return BadRequest(new { error = true, idSucursal = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Sucursal con ID {idRetornado} eliminada exitosamente.");
                        return Ok(new { error = false, idSucursal = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar sucursal: {ex.Message}");
                return StatusCode(500, new { error = true, idSucursal = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}