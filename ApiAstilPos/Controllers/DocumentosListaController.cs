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
    public class DocumentosListaController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DocumentosListaController> _logger;

        public DocumentosListaController(IConfiguration configuration, ILogger<DocumentosListaController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet]
        public async Task<IActionResult> GetDocumentosLista()
        {
            _logger.LogInformation("Obteniendo lista de documentos");

            try
            {
                var documentosLista = new List<DocumentoLista>();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_ventas", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonDocumentosLista = reader.IsDBNull(reader.GetOrdinal("ventas"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("ventas"));
                                documentosLista = JsonConvert.DeserializeObject<List<DocumentoLista>>(jsonDocumentosLista);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Documentos obtenidos: {documentosLista.Count}");
                return Ok(documentosLista);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener documentos: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}