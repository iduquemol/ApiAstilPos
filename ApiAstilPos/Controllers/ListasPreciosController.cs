using azureFunctionPos.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

        [HttpGet]
        public async Task<IActionResult> GetListasPrecios()
        {
            _logger.LogInformation("Obteniendo listas de precios");

            try
            {
                var listasPrecios = new List<ListaPrecio>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_listasPrecios", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonListasPrecios = reader.IsDBNull(reader.GetOrdinal("listasPrecios"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("listasPrecios"));
                                listasPrecios = JsonConvert.DeserializeObject<List<ListaPrecio>>(jsonListasPrecios);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Listas de Precios obtenidos: {listasPrecios.Count}");
                return Ok(listasPrecios);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener listas de precios: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}