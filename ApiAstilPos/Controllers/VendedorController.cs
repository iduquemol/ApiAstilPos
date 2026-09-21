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
    public class VendedoresController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VendedoresController> _logger;

        public VendedoresController(IConfiguration configuration, ILogger<VendedoresController> logger)
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

        [HttpGet("vendedores")]
        public async Task<IActionResult> GetVendedores()
        {
            _logger.LogInformation("Obteniendo lista de vendedores");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_vendedoresId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("vendedores");

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

                var vendedores = JsonConvert.DeserializeObject<List<Vendedor>>(jsonCompleto) ?? new List<Vendedor>();

                _logger.LogInformation($"Vendedores obtenidos: {vendedores.Count}");
                return Ok(vendedores);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener vendedores: {ex.Message}");
                return StatusCode(500, new { error = true, idVendedor = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("vendedores")]
        public async Task<IActionResult> CreateVendedor([FromBody] JsonElement vendedoresJson)
        {
            _logger.LogInformation("Creando un nuevo vendedor");

            try
            {
                string requestBody = vendedoresJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_vendedores", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@vendedores", requestBody ?? (object)DBNull.Value);

                        var paramIdVendedor = new SqlParameter("@idvendedor", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramIdVendedor);
                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = paramIdVendedor.Value != DBNull.Value ? Convert.ToInt64(paramIdVendedor.Value) : 0;
                        bool tieneError = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeTexto = ExtraerMensajeDb(paramMensaje.Value?.ToString(), "Vendedor creado correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al crear vendedor: {mensajeTexto}");
                            return BadRequest(new { error = true, idVendedor = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Vendedor creado con ID: {idRetornado}");
                        return Ok(new { error = false, idVendedor = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear vendedor: {ex.Message}");
                return StatusCode(500, new { error = true, idVendedor = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPut("vendedores")]
        public async Task<IActionResult> UpdateVendedor([FromBody] JsonElement vendedoresJson)
        {
            _logger.LogInformation("Actualizando un vendedor");

            try
            {
                string requestBody = vendedoresJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_vendedores", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@vendedores", requestBody ?? (object)DBNull.Value);

                        var paramIdVendedor = new SqlParameter("@idvendedor", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramIdVendedor);
                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = paramIdVendedor.Value != DBNull.Value ? Convert.ToInt64(paramIdVendedor.Value) : 0;
                        bool tieneError = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeTexto = ExtraerMensajeDb(paramMensaje.Value?.ToString(), "Vendedor actualizado correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al actualizar vendedor: {mensajeTexto}");
                            return BadRequest(new { error = true, idVendedor = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Vendedor con ID {idRetornado} actualizado correctamente.");
                        return Ok(new { error = false, idVendedor = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar vendedor: {ex.Message}");
                return StatusCode(500, new { error = true, idVendedor = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpDelete("vendedores/{id}")]
        public async Task<IActionResult> DeleteVendedor(long id)
        {
            _logger.LogInformation($"Intentando eliminar el vendedor con ID: {id}");

            try
            {
                string requestBody = JsonConvert.SerializeObject(new { idVendedor = id });

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_vendedores", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@vendedores", requestBody);

                        var paramIdVendedor = new SqlParameter("@idvendedor", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramIdVendedor);
                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = paramIdVendedor.Value != DBNull.Value ? Convert.ToInt64(paramIdVendedor.Value) : 0;
                        bool tieneError = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeTexto = ExtraerMensajeDb(paramMensaje.Value?.ToString(), "Vendedor eliminado correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al eliminar vendedor: {mensajeTexto}");
                            return BadRequest(new { error = true, idVendedor = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Vendedor con ID {idRetornado} eliminado exitosamente.");
                        return Ok(new { error = false, idVendedor = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar vendedor: {ex.Message}");
                return StatusCode(500, new { error = true, idVendedor = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}