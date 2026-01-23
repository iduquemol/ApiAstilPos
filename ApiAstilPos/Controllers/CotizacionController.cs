using ApiAstilPos.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CotizacionController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CotizacionController> _logger;

        public CotizacionController(IConfiguration configuration, ILogger<CotizacionController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpPost]
        public async Task<IActionResult> CreateCotizacion([FromBody] Cotizacion cotizacion)
        {
            _logger.LogInformation("Creando una nueva cotizacion");

            try
            {
                string requestBody = JsonConvert.SerializeObject(cotizacion);
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                object cotizacionId = null;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_cotizacion", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@cotizacion", requestBody ?? (object)DBNull.Value);

                        // Ejecutar el SP
                        cotizacionId = await command.ExecuteScalarAsync();

                        _logger.LogInformation("Cotizacion creada correctamente.");
                    }

                    if (cotizacionId == null || Convert.ToInt64(cotizacionId) <= 0)
                    {
                        _logger.LogError("No se pudo obtener un ID de cotizacion válido.");
                        return BadRequest("Error: No se pudo crear la cotizacion.");
                    }
                    else
                    {
                        return Ok(new { message = "Cotizacion creada correctamente", idCotizacion = cotizacionId });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear cotizacion: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}