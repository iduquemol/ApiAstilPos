using azureFunctionPos.Models;
using Microsoft.AspNetCore.Mvc;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ItemsController> _logger;

        public ItemsController(IConfiguration configuration, ILogger<ItemsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            _logger.LogInformation("Obteniendo lista de items");

            try
            {
                var items = new List<Producto>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_productos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                items.Add(new Producto
                                {
                                    IdProducto = reader.GetInt64(reader.GetOrdinal("idProducto")),
                                    CodigoProducto = reader.IsDBNull(reader.GetOrdinal("codigoProducto"))
                                        ? string.Empty
                                        : reader.GetString(reader.GetOrdinal("codigoProducto")),
                                    NombreProducto = reader.IsDBNull(reader.GetOrdinal("nombreProducto"))
                                        ? string.Empty
                                        : reader.GetString(reader.GetOrdinal("nombreProducto")),
                                    ImagenProducto = reader.IsDBNull(reader.GetOrdinal("imagenProducto"))
                                        ? string.Empty
                                        : reader.GetString(reader.GetOrdinal("imagenProducto")),
                                    CodigoBarras = reader.IsDBNull(reader.GetOrdinal("codigoBarras"))
                                        ? string.Empty
                                        : reader.GetString(reader.GetOrdinal("codigoBarras")),
                                    IdCategoria = reader.IsDBNull(reader.GetOrdinal("idCategoria"))
                                        ? 0
                                        : reader.GetInt64(reader.GetOrdinal("idCategoria")),
                                    IdUnidadMedida = reader.IsDBNull(reader.GetOrdinal("idUnidadMedida"))
                                        ? 0
                                        : reader.GetInt64(reader.GetOrdinal("idUnidadMedida")),
                                    PrecioUnitario = reader.IsDBNull(reader.GetOrdinal("precioUnitario"))
                                        ? 0M
                                        : reader.GetDecimal(reader.GetOrdinal("precioUnitario"))
                                });
                            }
                        }
                    }
                }

                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener items: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}