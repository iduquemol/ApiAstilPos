using azureFunctionPos.Models;
using Microsoft.AspNetCore.Mvc;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(IConfiguration configuration, ILogger<CategoriasController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetCategorias()
        {
            _logger.LogInformation("Obteniendo lista de categorias");

            try
            {
                var categorias = new List<Categoria>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_categorias", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                categorias.Add(new Categoria
                                {
                                    IdCategoria = reader.GetInt64(reader.GetOrdinal("idCategoria")),
                                    CodigoCategoria = reader.GetString(reader.GetOrdinal("codigoCategoria")),
                                    NombreCategoria = reader.GetString(reader.GetOrdinal("nombreCategoria")),
                                    IconoCategoria = reader.IsDBNull(reader.GetOrdinal("iconoCategoria"))
                                        ? string.Empty
                                        : reader.GetString(reader.GetOrdinal("iconoCategoria")),
                                });
                            }
                        }
                    }
                }
                _logger.LogInformation($"Categorias obtenidas: {categorias.Count}");
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener categorias: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}