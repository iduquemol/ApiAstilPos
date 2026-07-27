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
    public class ParametrosVentasController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ParametrosVentasController> _logger;

        public ParametrosVentasController(IConfiguration configuration, ILogger<ParametrosVentasController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("parametrosVentaDefault")]
        public async Task<IActionResult> GetParametrosVentasDefault()
        {
            _logger.LogInformation("Obteniendo datos por defecto para parámetros de ventas");

            try
            {
                var parametros = new ParametrosVentaDefault();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();

                    // 1. Lectura del Stored Procedure
                    using (var command = new SqlCommand("sp_Read_parametrosVentaDefault", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonParametros = reader.IsDBNull(reader.GetOrdinal("parametrosVenta"))
                                    ? "{}"
                                    : reader.GetString(reader.GetOrdinal("parametrosVenta"));

                                parametros = JsonConvert.DeserializeObject<ParametrosVentaDefault>(jsonParametros) ?? new ParametrosVentaDefault();
                            }
                        }
                    }

                    // 2. Consulta de listasPrecios
                    if (parametros.ListaPrecios == null || parametros.ListaPrecios.Length == 0)
                    {
                        var listaPreciosAux = new List<ListaPrecioDto>();
                        string queryListas = "SELECT idListaPrecio, nombreListaPrecio FROM dbo.listasPrecios";

                        using (var cmd = new SqlCommand(queryListas, connection))
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                listaPreciosAux.Add(new ListaPrecioDto
                                {
                                    IdListaPrecio = reader.GetInt64(0), // bigint
                                    NombreListaPrecio = reader.IsDBNull(1) ? "" : reader.GetString(1)
                                });
                            }
                        }
                        parametros.ListaPrecios = listaPreciosAux.ToArray();
                    }

                    // 3. Consulta de tiposRegimen
                    if (parametros.TipoRegimen == null || parametros.TipoRegimen.Length == 0)
                    {
                        var regimenAux = new List<TipoRegimenDto>();
                        string queryRegimen = "SELECT idTipoRegimen, nombreTipoRegimen FROM dbo.tiposRegimen";

                        using (var cmd = new SqlCommand(queryRegimen, connection))
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                regimenAux.Add(new TipoRegimenDto
                                {
                                    IdTipoRegimen = reader.GetInt64(0), // bigint
                                    NombreRegimen = reader.IsDBNull(1) ? "" : reader.GetString(1)
                                });
                            }
                        }
                        parametros.TipoRegimen = regimenAux.ToArray();
                    }
                }

                _logger.LogInformation("Parámetros de venta por defecto obtenidos correctamente.");
                return Ok(parametros);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener parámetros de ventas por defecto: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("parametrosVentas")]
        public async Task<IActionResult> CreateParametrosVentas([FromBody] JsonElement parametrosJson)
        {
            _logger.LogInformation("Creando parámetros de venta");

            try
            {
                string requestBody = parametrosJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_parametrosVentas", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@parametrosVentas", requestBody ?? (object)DBNull.Value);

                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Parámetros de venta creados correctamente.");
                        return Ok(new { message = "Parámetros de venta creados correctamente", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear parámetros de ventas: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPut("parametrosVentas")]
        public async Task<IActionResult> UpdateParametrosVentas([FromBody] JsonElement parametrosJson)
        {
            _logger.LogInformation("Actualizando parámetros de venta");

            try
            {
                string requestBody = parametrosJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_parametrosVentas", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@parametrosVentas", requestBody ?? (object)DBNull.Value);

                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Parámetros de venta actualizados correctamente.");
                        return Ok(new { message = "Parámetros de venta actualizados correctamente", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar parámetros de ventas: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}