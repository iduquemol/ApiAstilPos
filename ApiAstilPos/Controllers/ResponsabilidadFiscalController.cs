using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ApiAstilPos.Models;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/responsabilidadesfiscales")]
    public class ResponsabilidadFiscalController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ResponsabilidadFiscalController> _logger;

        public ResponsabilidadFiscalController(IConfiguration configuration, ILogger<ResponsabilidadFiscalController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetResponsabilidadesFiscales()
        {
            _logger.LogInformation("Obteniendo lista de responsabilidades fiscales");

            try
            {
                var responsabilidades = new List<ResponsabilidadFiscal>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_responsabilidadesFiscales", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonResponsabilidadFiscal = reader.IsDBNull(reader.GetOrdinal("responsabilidadesFiscales"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("responsabilidadesFiscales"));
                                responsabilidades = JsonConvert.DeserializeObject<List<ResponsabilidadFiscal>>(jsonResponsabilidadFiscal);
                            }
                        }
                    }
                }

                return Ok(responsabilidades);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener responsabilidades fiscales: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}