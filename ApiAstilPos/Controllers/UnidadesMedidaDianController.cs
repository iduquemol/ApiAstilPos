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
    public class UnidadesMedidaDianController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<UnidadesMedidaDianController> _logger;

        public UnidadesMedidaDianController(IConfiguration configuration, ILogger<UnidadesMedidaDianController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("unidadesmedidadian")]
        public async Task<IActionResult> GetUnidadesMedidaDian()
        {
            _logger.LogInformation("Obteniendo lista de unidades de medida Dian");

            try
            {
                var unidadesMedidaDian = new List<UnidadesMedidaDian>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_unidadesMedidaDianId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonUnidadesMedidaDian = reader.IsDBNull(reader.GetOrdinal("unidadesmedidadian"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("unidadesmedidadian"));

                                unidadesMedidaDian = JsonConvert.DeserializeObject<List<UnidadesMedidaDian>>(jsonUnidadesMedidaDian);
                            }
                        }
                    }
                }

                return Ok(unidadesMedidaDian);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener unidades de medida Dian: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
