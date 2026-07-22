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
    public class TiposDocumentoExternoController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TiposDocumentoExternoController> _logger;

        public TiposDocumentoExternoController(IConfiguration configuration, ILogger<TiposDocumentoExternoController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("tiposdocumentoexterno")]
        public async Task<IActionResult> GetTiposDocumentoExterno()
        {
            _logger.LogInformation("Obteniendo lista de tipos de documento externo");

            try
            {
                var tiposDocumento = new List<TipoDocumentoExterno>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_tiposDocumentoExterno", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonTiposDoc = reader.IsDBNull(reader.GetOrdinal("tipoDocumentoExterno"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("tipoDocumentoExterno"));
                                tiposDocumento = JsonConvert.DeserializeObject<List<TipoDocumentoExterno>>(jsonTiposDoc) ?? new List<TipoDocumentoExterno>();
                            }
                        }
                    }
                }

                return Ok(tiposDocumento);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener tipos de documento externo: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("tiposdocumentoexterno")]
        public async Task<IActionResult> CreateTipoDocumentoExterno([FromBody] JsonElement tiposDocumentoJson)
        {
            _logger.LogInformation("Creando un nuevo tipo de documento externo");

            try
            {
                string requestBody = tiposDocumentoJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_tiposDocumentoExterno", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@tiposDocumentoExterno", requestBody ?? (object)DBNull.Value);

                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Tipo de documento externo creado correctamente.");
                        return Ok(new { message = "Tipo de documento externo creado correctamente", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear tipo de documento externo: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPut("tiposdocumentoexterno")]
        public async Task<IActionResult> UpdateTipoDocumentoExterno([FromBody] JsonElement tiposDocumentoJson)
        {
            _logger.LogInformation("Actualizando un tipo de documento externo");

            try
            {
                string requestBody = tiposDocumentoJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_tiposDocumentoExterno", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@tiposDocumentoExterno", requestBody ?? (object)DBNull.Value);

                        var result = await command.ExecuteNonQueryAsync();

                        _logger.LogInformation("Tipo de documento externo actualizado correctamente.");
                        return Ok(new { message = "Tipo de documento externo actualizado correctamente", rowsAffected = result });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar tipo de documento externo: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete("tiposdocumentoexterno")]
        public async Task<IActionResult> DeleteTipoDocumentoExterno([FromBody] JObject request)
        {
            _logger.LogInformation("Borrando un tipo de documento externo");

            try
            {
                var idTipoDocumentoExterno = request["idTipoDocumentoExterno"]?.Value<long>() ?? 0;
                _logger.LogInformation($"ID Tipo Documento Externo a borrar: {idTipoDocumentoExterno}");
                var idBorrado = 0;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_tiposDocumentoExterno", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idTipoDocumentoExterno", idTipoDocumentoExterno);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                idBorrado = reader.IsDBNull(reader.GetOrdinal("idTipoDocumentoExterno"))
                                    ? 0
                                    : reader.GetInt32(reader.GetOrdinal("idTipoDocumentoExterno"));
                            }
                        }

                        _logger.LogInformation("Tipo de documento externo borrado correctamente.");
                        return Ok(new { message = "Tipo de documento externo borrado correctamente", idTipoDocumentoExternoBorrado = idBorrado });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al borrar tipo de documento externo: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}