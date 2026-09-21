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
    public class UsuariosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(IConfiguration configuration, ILogger<UsuariosController> logger)
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

        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            _logger.LogInformation("Obteniendo lista de usuarios");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_usuarios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("usuarios");

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

                var usuarios = JsonConvert.DeserializeObject<List<Usuario>>(jsonCompleto) ?? new List<Usuario>();

                _logger.LogInformation($"Usuarios obtenidos: {usuarios.Count}");
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener usuarios: {ex.Message}");
                return StatusCode(500, new { error = true, idUsuario = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("usuarios")]
        public async Task<IActionResult> CreateUsuario([FromBody] JsonElement usuariosJson)
        {
            _logger.LogInformation("Creando un nuevo usuario");

            try
            {
                string requestBody = usuariosJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_usuarios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@usuarios", requestBody ?? (object)DBNull.Value);

                        var paramIdUsuario = new SqlParameter("@idUsuario", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramIdUsuario);
                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = paramIdUsuario.Value != DBNull.Value ? Convert.ToInt64(paramIdUsuario.Value) : 0;
                        bool tieneError = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeTexto = ExtraerMensajeDb(paramMensaje.Value?.ToString(), "Usuario creado correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al crear usuario: {mensajeTexto}");
                            return BadRequest(new { error = true, idUsuario = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Usuario creado con ID: {idRetornado}");
                        return Ok(new { error = false, idUsuario = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear usuario: {ex.Message}");
                return StatusCode(500, new { error = true, idUsuario = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPut("usuarios")]
        public async Task<IActionResult> UpdateUsuario([FromBody] JsonElement usuariosJson)
        {
            _logger.LogInformation("Actualizando un usuario");

            try
            {
                string requestBody = usuariosJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_usuarios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // sp_Update_usuarios solo recibe 3 parámetros: @usuarios, @errorOutput, @mensajeOutput
                        command.Parameters.AddWithValue("@usuarios", requestBody ?? (object)DBNull.Value);

                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        long idRetornado = 0;
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                idRetornado = reader["idUsuario"] != DBNull.Value ? Convert.ToInt64(reader["idUsuario"]) : 0;
                            }
                        }

                        bool tieneError = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeTexto = ExtraerMensajeDb(paramMensaje.Value?.ToString(), "Usuario actualizado correctamente");

                        if (tieneError)
                        {
                            _logger.LogWarning($"Error al actualizar usuario: {mensajeTexto}");
                            return BadRequest(new { error = true, idUsuario = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Usuario con ID {idRetornado} actualizado correctamente.");
                        return Ok(new { error = false, idUsuario = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar usuario: {ex.Message}");
                return StatusCode(500, new { error = true, idUsuario = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpDelete("usuarios/{id}")]
        public async Task<IActionResult> DeleteUsuario(long id)
        {
            _logger.LogInformation($"Intentando eliminar el usuario con ID: {id}");

            try
            {
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_usuarios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // sp_Delete_usuarios recibe @idUsuario (bigint), NO @usuarios (nvarchar)
                        command.Parameters.AddWithValue("@idUsuario", id);

                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        long idRetornado = 0;
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                idRetornado = reader["idUsuario"] != DBNull.Value ? Convert.ToInt64(reader["idUsuario"]) : id;
                            }
                        }

                        bool tieneError = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeTexto = ExtraerMensajeDb(paramMensaje.Value?.ToString(), "Usuario eliminado correctamente");

                        if (tieneError)
                        {
                            _logger.LogWarning($"Error al eliminar usuario: {mensajeTexto}");
                            return BadRequest(new { error = true, idUsuario = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Usuario con ID {idRetornado} eliminado exitosamente.");
                        return Ok(new { error = false, idUsuario = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar usuario: {ex.Message}");
                return StatusCode(500, new { error = true, idUsuario = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}