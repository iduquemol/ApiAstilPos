using ApiAstilPos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class EventoDianController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EventoDianController> _logger;

        public EventoDianController(IConfiguration configuration, ILogger<EventoDianController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("eventos-dian")]
        public async Task<IActionResult> GetEventosDian([FromQuery] long? idEventoDian = null)
        {
            _logger.LogInformation("Obteniendo lista de eventos DIAN");

            try
            {
                var eventosDian = new List<EventosDian>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_eventosDianId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idEventoDian", (object?)idEventoDian ?? DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var json = reader.IsDBNull(reader.GetOrdinal("eventosDian"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("eventosDian"));

                                eventosDian = JsonConvert.DeserializeObject<List<EventosDian>>(json) ?? new List<EventosDian>();
                            }
                        }
                    }
                }

                _logger.LogInformation($"Eventos DIAN obtenidos: {eventosDian.Count}");
                return Ok(eventosDian);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener eventos DIAN");
                return StatusCode(500, new { error = true, idEventoDian = 0, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}