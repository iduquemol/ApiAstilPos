using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using System.Text;
using System.Text.Json;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class RolesOpcionesServicioController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RolesOpcionesServicioController> _logger;

        public RolesOpcionesServicioController(
            IConfiguration configuration,
            ILogger<RolesOpcionesServicioController> logger)
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

        /// <summary>
        /// Consulta los permisos de un rol para una opción específica y valida la acción solicitada.
        /// </summary>
        private async Task<bool> ValidarPermisoRolAsync(long idRol, long idOpcion, string accion)
        {
            var jsonBuilder = new StringBuilder();

            using (var connection = new SqlConnection(GetConnectionString()))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("sp_Read_rolesOpcionesServicioId", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@idRolOpcion", SqlDbType.BigInt).Value = DBNull.Value;
                    command.Parameters.Add("@idRol", SqlDbType.BigInt).Value = idRol;
                    command.Parameters.Add("@idOpcion", SqlDbType.BigInt).Value = idOpcion;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        int ordinal = reader.GetOrdinal("rolesOpcionesServicio");

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
                return false;

            var permisos = JsonConvert.DeserializeObject<List<RolOpcionServicio>>(jsonCompleto);
            var permiso = permisos?.FirstOrDefault();
            if (permiso == null)
                return false;

            return accion.Trim().ToLower() switch
            {
                "crear" => permiso.PuedeCrear,
                "leer" => permiso.PuedeLeer,
                "actualizar" => permiso.PuedeActualizar,
                "eliminar" => permiso.PuedeEliminar,
                _ => false
            };
        }

        [HttpGet("roles-opciones-servicio")]
        public async Task<IActionResult> GetRolesOpcionesServicio(
            [FromQuery] long? idRolOpcion = null,
            [FromQuery] long? idRol = null,
            [FromQuery] long? idOpcion = null)
        {
            _logger.LogInformation("Obteniendo permisos de roles y opciones de servicio");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_rolesOpcionesServicioId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@idRolOpcion", SqlDbType.BigInt).Value = (object?)idRolOpcion ?? DBNull.Value;
                        command.Parameters.Add("@idRol", SqlDbType.BigInt).Value = (object?)idRol ?? DBNull.Value;
                        command.Parameters.Add("@idOpcion", SqlDbType.BigInt).Value = (object?)idOpcion ?? DBNull.Value;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("rolesOpcionesServicio");

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

                var lista = JsonConvert.DeserializeObject<List<RolOpcionServicio>>(jsonCompleto)
                            ?? new List<RolOpcionServicio>();

                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener rolesOpcionesServicio: {ex.Message}");
                return StatusCode(500, new { error = true, idRolOpcion = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("roles-opciones-servicio")]
        public async Task<IActionResult> CreateRolOpcionServicio([FromBody] JsonElement bodyJson)
        {
            _logger.LogInformation("Guardando permiso de rol y opción de servicio");

            try
            {
                string requestBody = bodyJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_rolesOpcionesServicio", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@rolesOpcionesServicio", SqlDbType.NVarChar, -1).Value =
                            string.IsNullOrWhiteSpace(requestBody) ? DBNull.Value : requestBody;

                        var paramId = new SqlParameter("@idRolOpcion", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramId);
                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = paramId.Value != DBNull.Value ? Convert.ToInt64(paramId.Value) : 0;
                        bool tieneError = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeTexto = ExtraerMensajeDb(paramMensaje.Value?.ToString(), "Permiso guardado correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al guardar permiso: {mensajeTexto}");
                            return BadRequest(new { error = true, idRolOpcion = idRetornado, mensaje = mensajeTexto });
                        }

                        return Ok(new { error = false, idRolOpcion = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear rolesOpcionesServicio: {ex.Message}");
                return StatusCode(500, new { error = true, idRolOpcion = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPut("roles-opciones-servicio")]
        public async Task<IActionResult> UpdateRolOpcionServicio([FromBody] JsonElement bodyJson)
        {
            _logger.LogInformation("Actualizando permiso de rol y opción de servicio");

            try
            {
                string requestBody = bodyJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_rolesOpcionesServicio", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@rolesOpcionesServicio", SqlDbType.NVarChar, -1).Value =
                            string.IsNullOrWhiteSpace(requestBody) ? DBNull.Value : requestBody;

                        var paramId = new SqlParameter("@idRolOpcion", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramId);
                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = paramId.Value != DBNull.Value ? Convert.ToInt64(paramId.Value) : 0;
                        bool tieneError = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeTexto = ExtraerMensajeDb(paramMensaje.Value?.ToString(), "Permiso actualizado correctamente");

                        if (tieneError)
                        {
                            _logger.LogWarning($"Error al actualizar permiso: {mensajeTexto}");
                            return BadRequest(new { error = true, idRolOpcion = idRetornado, mensaje = mensajeTexto });
                        }

                        return Ok(new { error = false, idRolOpcion = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar rolesOpcionesServicio: {ex.Message}");
                return StatusCode(500, new { error = true, idRolOpcion = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpDelete("roles-opciones-servicio/{id}")]
        public async Task<IActionResult> DeleteRolOpcionServicio(long id)
        {
            _logger.LogInformation($"Eliminando permiso con ID: {id}");

            try
            {
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_rolesOpcionesServicioId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@idRolOpcion", SqlDbType.BigInt).Value = id;

                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        await command.ExecuteNonQueryAsync();

                        bool tieneError = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeTexto = ExtraerMensajeDb(paramMensaje.Value?.ToString(), "Permiso eliminado correctamente");

                        if (tieneError)
                        {
                            _logger.LogWarning($"Error al eliminar permiso: {mensajeTexto}");
                            return BadRequest(new { error = true, idRolOpcion = id, mensaje = mensajeTexto });
                        }

                        return Ok(new { error = false, idRolOpcion = id, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar rolesOpcionesServicio: {ex.Message}");
                return StatusCode(500, new { error = true, idRolOpcion = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}