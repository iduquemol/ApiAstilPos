using ApiAstilPos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text.Json;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class CategoriasController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(IConfiguration configuration, ILogger<CategoriasController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString() => _configuration.GetConnectionString("SqlConnectionString")!;

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

        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias([FromQuery] long? idCategoria = null)
        {
            _logger.LogInformation("Obteniendo lista de categorías");

            try
            {
                var categorias = new List<Categorias>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_categoriasId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idCategoria", (object?)idCategoria ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var json = reader.IsDBNull(reader.GetOrdinal("categorias"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("categorias"));

                                categorias = JsonConvert.DeserializeObject<List<Categorias>>(json) ?? new List<Categorias>();
                            }
                        }
                    }
                }

                _logger.LogInformation($"Categorías obtenidas: {categorias.Count}");
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                return StatusCode(500, new { error = true, idCategoria = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost("categorias")]
        public async Task<IActionResult> CreateCategoria([FromBody] JsonElement categoriaJson)
        {
            _logger.LogInformation("Creando una nueva categoría");

            try
            {
                string requestBody = categoriaJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_categorias", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@categorias", SqlDbType.NVarChar, -1)
                        {
                            Value = requestBody ?? (object)DBNull.Value
                        });

                        var errorOutputParam = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var mensajeOutputParam = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(errorOutputParam);
                        command.Parameters.Add(mensajeOutputParam);

                        await command.ExecuteNonQueryAsync();

                        bool tieneError = errorOutputParam.Value != DBNull.Value && Convert.ToBoolean(errorOutputParam.Value);
                        string mensajeTexto = ExtraerMensajeDb(mensajeOutputParam.Value?.ToString(), "Categoría creada correctamente.");

                        if (tieneError)
                        {
                            _logger.LogWarning($"Error al crear categoría: {mensajeTexto}");
                            return BadRequest(new { error = true, idCategoria = 0, mensaje = mensajeTexto });
                        }

                        _logger.LogInformation("Categoría creada exitosamente.");
                        return Ok(new { error = false, idCategoria = 0, mensaje = mensajeTexto });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría");
                return StatusCode(500, new { error = true, idCategoria = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPut("categorias")]
        public async Task<IActionResult> UpdateCategoria([FromBody] JsonElement categoriaJson)
        {
            _logger.LogInformation("Actualizando categoría");

            try
            {
                string requestBody = categoriaJson.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_categorias", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@categorias", SqlDbType.NVarChar, -1)
                        {
                            Value = requestBody ?? (object)DBNull.Value
                        });

                        await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Categoría actualizada correctamente.");
                        return Ok(new { error = false, idCategoria = 0, mensaje = "Categoría actualizada correctamente." });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría");
                return StatusCode(500, new { error = true, idCategoria = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpDelete("categorias/{id}")]
        public async Task<IActionResult> DeleteCategoria(long id)
        {
            _logger.LogInformation($"Intentando eliminar categoría con ID: {id}");

            if (id <= 0)
            {
                return BadRequest(new { error = true, idCategoria = id, mensaje = "El ID de categoría proporcionado no es válido." });
            }

            try
            {
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_categorias", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idCategoria", id);

                        await command.ExecuteNonQueryAsync();

                        _logger.LogInformation($"Categoría con ID {id} eliminada exitosamente.");
                        return Ok(new { error = false, idCategoria = id, mensaje = "Categoría eliminada correctamente." });
                    }
                }
            }
            catch (SqlException ex)
            {
                // Captura el mensaje limpio de SQL Server cuando salta por THROW o RAISERROR
                _logger.LogWarning(ex, $"Error SQL al eliminar categoría con ID {id}: {ex.Message}");
                return BadRequest(new { error = true, idCategoria = id, mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error no controlado al eliminar categoría con ID {id}");
                return StatusCode(500, new { error = true, idCategoria = id, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}