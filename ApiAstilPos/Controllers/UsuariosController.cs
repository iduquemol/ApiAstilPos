using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;

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
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            _logger.LogInformation("Obteniendo lista de usuarios");

            try
            {
                var usuarios = new List<Usuario>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_usuarios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonUsuarios = reader.IsDBNull(reader.GetOrdinal("usuarios"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("usuarios"));

                                usuarios = JsonConvert.DeserializeObject<List<Usuario>>(jsonUsuarios);
                            }
                        }
                    }
                }

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener usuarios: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("usuarios")]
        public async Task<IActionResult> CreateUsuario([FromBody] JsonElement usuariosJson)
        {
            _logger.LogInformation("Creando un nuevo usuario");

            try
            {
                string requestBody = usuariosJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

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

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                /* Loop para hidratar parámetros OUTPUT */
                            }
                        }

                        long idUsuario = paramIdUsuario.Value != DBNull.Value ? Convert.ToInt64(paramIdUsuario.Value) : 0;
                        bool errorOutput = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeOutput = paramMensaje.Value?.ToString() ?? string.Empty;

                        _logger.LogInformation($"Procedimiento ejecutado. ID: {idUsuario}, Error: {errorOutput}");

                        if (errorOutput)
                        {
                            return BadRequest(new
                            {
                                error = true,
                                idUsuario,
                                mensaje = mensajeOutput
                            });
                        }

                        return Ok(new
                        {
                            error = false,
                            idUsuario,
                            mensaje = mensajeOutput
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear usuario: {ex.Message}");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPut("usuarios")]
        public async Task<IActionResult> UpdateUsuario([FromBody] JsonElement usuariosJson)
        {
            _logger.LogInformation("Actualizando un usuario");

            try
            {
                string requestBody = usuariosJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_usuarios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@usuarios", requestBody ?? (object)DBNull.Value);

                        var pErrorOutput = command.Parameters.Add("@errorOutput", SqlDbType.Bit);
                        pErrorOutput.Direction = ParameterDirection.Output;

                        var pMensajeOutput = command.Parameters.Add("@mensajeOutput", SqlDbType.NVarChar, -1);
                        pMensajeOutput.Direction = ParameterDirection.Output;

                        long idUsuarioEditado = 0;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                idUsuarioEditado = reader.IsDBNull(reader.GetOrdinal("idUsuario"))
                                    ? 0
                                    : reader.GetInt64(reader.GetOrdinal("idUsuario"));
                            }
                        }

                        bool hasError = pErrorOutput.Value != DBNull.Value && (bool)pErrorOutput.Value;
                        string mensaje = pMensajeOutput.Value?.ToString() ?? "Operación completada";

                        if (hasError)
                        {
                            return BadRequest(new { error = true, mensaje });
                        }

                        return Ok(new { message = mensaje, idUsuario = idUsuarioEditado });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar usuario: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete("usuarios/{id}")]
        public async Task<IActionResult> DeleteUsuario(long id)
        {
            _logger.LogInformation($"Borrando usuario con ID: {id}");

            try
            {
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_usuarios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@idUsuario", id);

                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        long idUsuarioBorrado = 0;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                idUsuarioBorrado = reader.IsDBNull(reader.GetOrdinal("idUsuario"))
                                    ? 0
                                    : reader.GetInt64(reader.GetOrdinal("idUsuario"));
                            }
                        }

                        bool errorOutput = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeOutput = paramMensaje.Value?.ToString() ?? string.Empty;

                        _logger.LogInformation($"Procedimiento Delete ejecutado. ID: {idUsuarioBorrado}, Error: {errorOutput}");

                        if (errorOutput)
                        {
                            return BadRequest(new
                            {
                                error = true,
                                idUsuario = idUsuarioBorrado,
                                mensaje = mensajeOutput
                            });
                        }

                        return Ok(new
                        {
                            error = false,
                            idUsuarioBorrado,
                            mensaje = mensajeOutput
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al borrar usuario: {ex.Message}");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}