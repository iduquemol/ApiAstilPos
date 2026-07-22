using ApiAstilPos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                _logger.LogError(ex, "Error al obtener categorias");
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
                        command.Parameters.AddWithValue("@categorias", requestBody);
                        await command.ExecuteNonQueryAsync();
                        return Ok(new { message = "Categoría creada correctamente." });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
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
                        command.Parameters.AddWithValue("@categorias", requestBody);
                        await command.ExecuteNonQueryAsync();
                        return Ok(new { message = "Categoría actualizada correctamente." });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete("categorias")]
        public async Task<IActionResult> DeleteCategoria([FromBody] JObject request)
        {
            try
            {
                var idCategoria = request["idCategoria"]?.Value<long>() ?? 0;
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
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}