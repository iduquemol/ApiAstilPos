using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/tiposproducto")]
    public class TipoProductoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TipoProductoController> _logger;

        public TipoProductoController(IConfiguration configuration, ILogger<TipoProductoController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetTiposProducto()
        {
            _logger.LogInformation("Obteniendo lista de tipos de producto");

            try
            {
                var tiposProducto = new List<TipoProducto>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposProductos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonTiposProducto = reader.IsDBNull(reader.GetOrdinal("tiposProductos"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("tiposProductos"));
                                tiposProducto = JsonConvert.DeserializeObject<List<TipoProducto>>(jsonTiposProducto);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Tipos de producto obtenidos: {tiposProducto.Count}");
                return Ok(tiposProducto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tipos de producto: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}