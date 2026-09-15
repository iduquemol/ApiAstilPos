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
    public class UnidadDeMedidaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UnidadDeMedidaController> _logger;

        public UnidadDeMedidaController(IConfiguration configuration, ILogger<UnidadDeMedidaController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("unidadesdemedida")]
        public async Task<IActionResult> GetUnidadesMedidaDian()
        {
            _logger.LogInformation("Obteniendo lista de unidades de medida");

            try
            {
                var unidadDeMedida = new List<UnidadDeMedida>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_unidadesMedidaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonUnidadDeMedida = reader.IsDBNull(reader.GetOrdinal("unidadesMedida"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("unidadesMedida"));

                                unidadDeMedida = JsonConvert.DeserializeObject<List<UnidadDeMedida>>(jsonUnidadDeMedida);
                            }
                        }
                    }
                }

                return Ok(unidadDeMedida);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener unidades de medida: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("unidadesdemedida")]
        public async Task<IActionResult> CreateUnidadesDeMedida([FromBody] JsonElement unidadesDeMedidaJson)
        {
            _logger.LogInformation("Creando una nueva unidad de medida");

            try
            {
                string requestBody = unidadesDeMedidaJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_unidadesMedida", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@unidadesMedida", requestBody ?? (object)DBNull.Value);

                        var idUnidadMedidaParam = new SqlParameter("@idUnidadMedida", SqlDbType.BigInt)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };

                        command.Parameters.Add(idUnidadMedidaParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idUnidadMedidaParam.Value != DBNull.Value ? (long)idUnidadMedidaParam.Value : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && (bool)errorOutputParam.Value;
                        string mensaje = mensajeOutputParam.Value != DBNull.Value ? mensajeOutputParam.Value.ToString() : string.Empty;

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al crear unidad de medida: {mensaje}");
                            return BadRequest(string.IsNullOrEmpty(mensaje) ? "No se pudo crear la unidad de medida." : mensaje);
                        }

                        _logger.LogInformation($"Unidad de medida creada con ID: {idRetornado}");
                        return Ok(new { message = string.IsNullOrEmpty(mensaje) ? "Unidad de medida creada correctamente" : mensaje, idUnidadMedida = idRetornado });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear unidad de medida: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPut("unidadesdemedida")]
        public async Task<IActionResult> UpdateUnidadesDeMedida([FromBody] JsonElement unidadesDeMedidaJson)
        {
            _logger.LogInformation("Actualizando una unidad de medida");

            try
            {
                string requestBody = unidadesDeMedidaJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_unidadesMedida", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@unidadesMedida", requestBody ?? (object)DBNull.Value);

                        var idUnidadMedidaParam = new SqlParameter("@idUnidadMedida", SqlDbType.BigInt)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };

                        command.Parameters.Add(idUnidadMedidaParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idUnidadMedidaParam.Value != DBNull.Value ? (long)idUnidadMedidaParam.Value : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && (bool)errorOutputParam.Value;
                        string mensaje = mensajeOutputParam.Value != DBNull.Value ? mensajeOutputParam.Value.ToString() : string.Empty;

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al actualizar unidad de medida: {mensaje}");
                            return BadRequest(string.IsNullOrEmpty(mensaje) ? "No se pudo actualizar la unidad de medida." : mensaje);
                        }

                        _logger.LogInformation($"Unidad de medida con ID {idRetornado} actualizada correctamente.");
                        return Ok(new { message = string.IsNullOrEmpty(mensaje) ? "UNIDAD DE MEDIDA actualizada correctamente" : mensaje, idUnidadMedida = idRetornado });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar unidad de medida: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete("unidadesdemedida/{id}")]
        public async Task<IActionResult> DeleteUnidadesDeMedida(long id)
        {
            _logger.LogInformation($"Intentando eliminar la unidad de medida con ID: {id}");

            try
            {
                string requestBody = JsonConvert.SerializeObject(new { idUnidadMedida = id });

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_unidadesMedida", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@unidadesMedida", requestBody);

                        var idUnidadMedidaParam = new SqlParameter("@idUnidadMedida", SqlDbType.BigInt)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit)
                        {
                            Direction = ParameterDirection.Output
                        };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };

                        command.Parameters.Add(idUnidadMedidaParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idUnidadMedidaParam.Value != DBNull.Value ? (long)idUnidadMedidaParam.Value : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && (bool)errorOutputParam.Value;
                        string mensajeRaw = mensajeOutputParam.Value != DBNull.Value ? mensajeOutputParam.Value.ToString() : string.Empty;

                        // Extraemos el texto del JSON que retorna el SP en @mensajeOutput
                        string mensajeTexto = "Unidad de medida eliminada correctamente";
                        if (!string.IsNullOrWhiteSpace(mensajeRaw))
                        {
                            try
                            {
                                var listaMensajes = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(mensajeRaw);
                                if (listaMensajes != null && listaMensajes.Count > 0 && listaMensajes[0].ContainsKey("mensaje"))
                                {
                                    mensajeTexto = listaMensajes[0]["mensaje"]?.ToString() ?? mensajeTexto;
                                }
                            }
                            catch
                            {
                                mensajeTexto = mensajeRaw;
                            }
                        }

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al eliminar unidad de medida: {mensajeTexto}");
                            return BadRequest(mensajeTexto);
                        }

                        _logger.LogInformation($"Unidad de medida con ID {idRetornado} eliminada exitosamente.");

                        // Retornamos ambas claves para compatibilidad absoluta con cualquier interfaz del frontend
                        return Ok(new
                        {
                            message = mensajeTexto,
                            mensaje = mensajeTexto,
                            idUnidadMedida = idRetornado
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar unidad de medida: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
