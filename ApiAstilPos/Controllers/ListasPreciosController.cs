using ApiAstilPos.Models;
using azureFunctionPos.Models;
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
    public class ListasPreciosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ListasPreciosController> _logger;

        public ListasPreciosController(IConfiguration configuration, ILogger<ListasPreciosController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("listasPrecios")]
        public async Task<IActionResult> GetListasPrecios()
        {
            _logger.LogInformation("Obteniendo listas de precios");

            try
            {
                var jsonBuilder = new StringBuilder();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_listasPreciosId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            int ordinal = reader.GetOrdinal("listasPrecios");

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

                var listasPrecios = JsonConvert.DeserializeObject<List<ListaPrecio>>(jsonCompleto) ?? new List<ListaPrecio>();

                _logger.LogInformation($"Listas de precios obtenidas: {listasPrecios.Count}");
                return Ok(listasPrecios);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener listas de precios: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("listasPrecios")]
        public async Task<IActionResult> CreateListaPrecio([FromBody] JsonElement listasPreciosJson)
        {
            _logger.LogInformation("Creando una nueva lista de precios");

            try
            {
                string requestBody = listasPreciosJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_listasPrecios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@listasPrecios", requestBody ?? (object)DBNull.Value);

                        var idListaPrecioParam = new SqlParameter("@idListaPrecio", SqlDbType.BigInt)
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

                        command.Parameters.Add(idListaPrecioParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idListaPrecioParam.Value != DBNull.Value ? (long)idListaPrecioParam.Value : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && (bool)errorOutputParam.Value;
                        string mensaje = mensajeOutputParam.Value != DBNull.Value ? mensajeOutputParam.Value.ToString() : string.Empty;

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al crear lista de precios: {mensaje}");
                            return BadRequest(string.IsNullOrEmpty(mensaje) ? "No se pudo crear la lista de precios." : mensaje);
                        }

                        _logger.LogInformation($"Lista de precios creada con ID: {idRetornado}");
                        return Ok(new { message = string.IsNullOrEmpty(mensaje) ? "Lista de precios creada correctamente" : mensaje, idListaPrecio = idRetornado });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear lista de precios: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPut("listasPrecios")]
        public async Task<IActionResult> UpdateListaPrecio([FromBody] JsonElement listasPreciosJson)
        {
            _logger.LogInformation("Actualizando una lista de precios");

            try
            {
                string requestBody = listasPreciosJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_listasPrecios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@listasPrecios", requestBody ?? (object)DBNull.Value);

                        var idListaPrecioParam = new SqlParameter("@idListaPrecio", SqlDbType.BigInt)
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

                        command.Parameters.Add(idListaPrecioParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idListaPrecioParam.Value != DBNull.Value ? (long)idListaPrecioParam.Value : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && (bool)errorOutputParam.Value;
                        string mensaje = mensajeOutputParam.Value != DBNull.Value ? mensajeOutputParam.Value.ToString() : string.Empty;

                        if (tieneError || idRetornado == 0)
                        {
                            _logger.LogWarning($"Error al actualizar lista de precios: {mensaje}");
                            return BadRequest(string.IsNullOrEmpty(mensaje) ? "No se pudo actualizar la lista de precios." : mensaje);
                        }

                        _logger.LogInformation($"Lista de precios con ID {idRetornado} actualizada correctamente.");
                        return Ok(new { message = string.IsNullOrEmpty(mensaje) ? "Lista de precios actualizada correctamente" : mensaje, idListaPrecio = idRetornado });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar lista de precios: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete("listasPrecios/{id}")]
        public async Task<IActionResult> DeleteListaPrecio(long id)
        {
            _logger.LogInformation($"Intentando eliminar la lista de precios con ID: {id}");

            try
            {
                string requestBody = JsonConvert.SerializeObject(new { idListaPrecio = id });

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_listasPrecios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@listasPrecios", requestBody);

                        var idListaPrecioParam = new SqlParameter("@idListaPrecio", SqlDbType.BigInt)
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

                        command.Parameters.Add(idListaPrecioParam);
                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        long idRetornado = idListaPrecioParam.Value != DBNull.Value ? (long)idListaPrecioParam.Value : 0;
                        bool tieneError = errorOutputParam.Value != DBNull.Value && (bool)errorOutputParam.Value;
                        string mensajeRaw = mensajeOutputParam.Value != DBNull.Value ? mensajeOutputParam.Value.ToString() : string.Empty;

                        string mensajeTexto = "Lista de precios eliminada correctamente";
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
                            _logger.LogWarning($"Error al eliminar lista de precios: {mensajeTexto}");
                            return BadRequest(mensajeTexto);
                        }

                        _logger.LogInformation($"Lista de precios con ID {idRetornado} eliminada exitosamente.");

                        return Ok(new
                        {
                            message = mensajeTexto,
                            mensaje = mensajeTexto,
                            idListaPrecio = idRetornado
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar lista de precios: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}