using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Text.Json;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class ProductosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductosController> _logger;

        public ProductosController(IConfiguration configuration, ILogger<ProductosController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("productos")]
        public async Task<IActionResult> GetProductos()
        {
            _logger.LogInformation("Obteniendo lista de productos");

            try
            {
                var productos = new List<Producto>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_productos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonProductos = reader.IsDBNull(reader.GetOrdinal("productos"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("productos"));
                                productos = JsonConvert.DeserializeObject<List<Producto>>(jsonProductos);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Productos obtenidos: {productos.Count}");
                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener productos: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("productos-venta-tercero")]
        public async Task<IActionResult> PostProductosVentaTercero([FromBody] JsonElement request)
        {
            _logger.LogInformation("Procesando solicitud para productos por tercero");

            try
            {
                var idTercero = request.TryGetProperty("tercero", out var terceroElement)
                    ? terceroElement.GetString() ?? string.Empty
                    : string.Empty;

                _logger.LogInformation($"ID Tercero: {idTercero}");

                var productos = new List<Producto>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_productosVenta", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idTercero", idTercero);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonProductos = reader.IsDBNull(reader.GetOrdinal("productosVenta"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("productosVenta"));
                                productos = JsonConvert.DeserializeObject<List<Producto>>(jsonProductos);
                            }
                        }

                        _logger.LogInformation("Productos por tercero obtenidos correctamente.");
                        return Ok(productos);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener productos por tercero: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("productos")]
        public async Task<IActionResult> CreateProducto([FromBody] JsonElement productosJson)
        {
            _logger.LogInformation("Creando un nuevo producto");

            try
            {
                string requestBody = productosJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_productos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@productos", requestBody ?? (object)DBNull.Value);

                        // Ejecutar el SP
                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Producto creado correctamente.");
                        return Ok(new { message = "Producto creado correctamente", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear producto: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPut("productos")]
        public async Task<IActionResult> UpdateProducto([FromBody] JsonElement productosJson)
        {
            _logger.LogInformation("Actualizando un producto");

            try
            {
                string requestBody = productosJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_productos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@productos", requestBody ?? (object)DBNull.Value);

                        // Ejecutar el SP
                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Producto actualizado correctamente.");
                        return Ok(new { message = "Producto actualizado correctamente", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar producto: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}