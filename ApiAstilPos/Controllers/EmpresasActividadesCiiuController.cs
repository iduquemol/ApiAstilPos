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
    public class EmpresasActividadesCiiuController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmpresasActividadesCiiuController> _logger;

        public EmpresasActividadesCiiuController(IConfiguration configuration, ILogger<EmpresasActividadesCiiuController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
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

        [HttpGet("empresas-actividades-ciiu")]
        public async Task<IActionResult> GetEmpresasActividadesCiiu([FromQuery] long? idEmpresaActividadCiiu)
        {
            _logger.LogInformation("Obteniendo lista de relaciones empresas y actividades CIIU");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_empresasActividadesCiiuId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idEmpresaActividadCiiu", (object)idEmpresaActividadCiiu ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("empresasActividadesCiiu");

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

                var lista = JsonConvert.DeserializeObject<List<EmpresasActividadesCiiu>>(jsonCompleto) ?? new List<EmpresasActividadesCiiu>();

                _logger.LogInformation($"Registros obtenidos: {lista.Count}");
                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener empresasActividadesCiiu: {ex.Message}");
                return StatusCode(500, new { error = true, idEmpresaActividadCiiu = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("empresas-actividades-ciiu")]
        public async Task<IActionResult> CreateEmpresaActividadCiiu([FromBody] JsonElement entidadJson)
        {
            _logger.LogInformation("Creando una nueva relación empresa-actividad CIIU");

            try
            {
                string requestBody = entidadJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_empresasActividadesCiiu", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@empresasActividadesCiiu", requestBody ?? (object)DBNull.Value);

                        var idParam = new SqlParameter("@idEmpresaActividadCiiu", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idParam);
                        command.Parameters.Add(errorParam);
                        command.Parameters.Add(mensajeParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idParam.Value != DBNull.Value ? Convert.ToInt64(idParam.Value) : 0;
                        bool tieneError = errorParam.Value != DBNull.Value && Convert.ToBoolean(errorParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeParam.Value?.ToString(), "Registro creado correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al crear relación empresa-actividad: {mensajeTexto}");
                            return BadRequest(new { error = true, idEmpresaActividadCiiu = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Relación creada con ID: {idRetornado}");
                        return Ok(new { error = false, idEmpresaActividadCiiu = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear relación empresa-actividad: {ex.Message}");
                return StatusCode(500, new { error = true, idEmpresaActividadCiiu = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPut("empresas-actividades-ciiu")]
        public async Task<IActionResult> UpdateEmpresaActividadCiiu([FromBody] JsonElement entidadJson)
        {
            _logger.LogInformation("Actualizando una relación empresa-actividad CIIU");

            try
            {
                string requestBody = entidadJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    
                    using (var command = new SqlCommand("sp_Update_empresasActividadesCiiu", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@empresasActividadesCiiu", requestBody ?? (object)DBNull.Value);

                        var idParam = new SqlParameter("@idEmpresaActividadCiiu", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idParam);
                        command.Parameters.Add(errorParam);
                        command.Parameters.Add(mensajeParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idParam.Value != DBNull.Value ? Convert.ToInt64(idParam.Value) : 0;
                        bool tieneError = errorParam.Value != DBNull.Value && Convert.ToBoolean(errorParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeParam.Value?.ToString(), "Registro actualizado correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al actualizar relación empresa-actividad: {mensajeTexto}");
                            return BadRequest(new { error = true, idEmpresaActividadCiiu = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Registro con ID {idRetornado} actualizado correctamente.");
                        return Ok(new { error = false, idEmpresaActividadCiiu = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar relación empresa-actividad: {ex.Message}");
                return StatusCode(500, new { error = true, idEmpresaActividadCiiu = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpDelete("empresas-actividades-ciiu/{id}")]
        public async Task<IActionResult> DeleteEmpresaActividadCiiu(long id)
        {
            _logger.LogInformation($"Intentando eliminar la relación empresa-actividad con ID: {id}");

            try
            {
                string requestBody = JsonConvert.SerializeObject(new { idEmpresaActividadCiiu = id });

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    
                    using (var command = new SqlCommand("sp_Delete_empresasActividadesCiiu", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@empresasActividadesCiiu", requestBody);

                        var idParam = new SqlParameter("@idEmpresaActividadCiiu", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var errorParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(idParam);
                        command.Parameters.Add(errorParam);
                        command.Parameters.Add(mensajeParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idParam.Value != DBNull.Value ? Convert.ToInt64(idParam.Value) : 0;
                        bool tieneError = errorParam.Value != DBNull.Value && Convert.ToBoolean(errorParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeParam.Value?.ToString(), "Registro eliminado correctamente");

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al eliminar relación empresa-actividad: {mensajeTexto}");
                            return BadRequest(new { error = true, idEmpresaActividadCiiu = idRetornado, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation($"Registro con ID {idRetornado} eliminado exitosamente.");
                        return Ok(new { error = false, idEmpresaActividadCiiu = idRetornado, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar relación empresa-actividad: {ex.Message}");
                return StatusCode(500, new { error = true, idEmpresaActividadCiiu = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}