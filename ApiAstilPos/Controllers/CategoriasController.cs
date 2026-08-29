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

        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias()
        {
            try
            {
                var categorias = new List<Categorias>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_categorias", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var json = reader.IsDBNull(reader.GetOrdinal("categorias"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("categorias"));

                                categorias = JsonConvert.DeserializeObject<List<Categorias>>(json) ?? new List<Categorias>();
                            }
                        }
                    }
                }
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpPost("categorias")]
        public async Task<IActionResult> CreateCategoria([FromBody] JsonElement categoriaJson)
        {
            try
            {
                string requestBody = categoriaJson.GetRawText();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_categorias", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Asignación explícita para evitar truncamientos en NVARCHAR(MAX)
                        command.Parameters.Add(new SqlParameter("@categorias", SqlDbType.NVarChar, -1)
                        {
                            Value = requestBody
                        });

                        await command.ExecuteNonQueryAsync();
                        return Ok(new { message = "Categoría creada correctamente." });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("categorias")]
        public async Task<IActionResult> UpdateCategoria([FromBody] JsonElement categoriaJson)
        {
            try
            {
                string requestBody = categoriaJson.GetRawText();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_categorias", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Asignación explícita para evitar truncamientos en NVARCHAR(MAX)
                        command.Parameters.Add(new SqlParameter("@categorias", SqlDbType.NVarChar, -1)
                        {
                            Value = requestBody
                        });

                        await command.ExecuteNonQueryAsync();
                        return Ok(new { message = "Categoría actualizada correctamente." });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("categorias")]
        public async Task<IActionResult> DeleteCategoria([FromBody] JsonElement request)
        {
            try
            {
                long idCategoria = 0;

                if (request.TryGetProperty("idCategoria", out JsonElement idElement))
                {
                    idCategoria = idElement.GetInt64();
                }

                if (idCategoria == 0)
                {
                    return BadRequest("El idCategoria enviado no es válido.");
                }

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_categorias", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idCategoria", idCategoria);
                        await command.ExecuteNonQueryAsync();
                        return Ok(new { message = "Categoría eliminada correctamente." });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría");
                return BadRequest(ex.Message);
            }
        }
    }
}