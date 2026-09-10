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
                    using (var command = new SqlCommand("sp_Read_tiposDocumentoExternoId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var ordinal = reader.GetOrdinal("tiposDocumentoExterno");
                                var jsonTiposDoc = reader.IsDBNull(ordinal)
                                    ? "[]"
                                    : reader.GetString(ordinal);

                                tiposDocumento = JsonConvert.DeserializeObject<List<TipoDocumentoExterno>>(jsonTiposDoc)
                                                 ?? new List<TipoDocumentoExterno>();
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

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_tiposDocumentoExterno", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@tiposDocumentoExterno", requestBody ?? (object)DBNull.Value);

                        // Parámetros OUTPUT según el SP
                        var paramId = new SqlParameter("@idTipoDocumentoExterno", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramId);
                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        await command.ExecuteNonQueryAsync();

                        bool error = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensaje = paramMensaje.Value?.ToString() ?? string.Empty;

                        if (error)
                        {
                            return BadRequest(new { message = "Error al crear en base de datos", detalle = mensaje });
                        }

                        long newId = paramId.Value != DBNull.Value ? Convert.ToInt64(paramId.Value) : 0;
                        return Ok(new { message = "Tipo de documento externo creado correctamente", idTipoDocumentoExterno = newId });
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

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_tiposDocumentoExterno", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@tiposDocumentoExterno", requestBody ?? (object)DBNull.Value);

                        var paramId = new SqlParameter("@idTipoDocumentoExterno", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramId);
                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        await command.ExecuteNonQueryAsync();

                        bool error = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensaje = paramMensaje.Value?.ToString() ?? string.Empty;

                        if (error)
                        {
                            return BadRequest(new { message = "Error al actualizar en base de datos", detalle = mensaje });
                        }

                        return Ok(new { message = "Tipo de documento externo actualizado correctamente" });
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
        public async Task<IActionResult> DeleteTipoDocumentoExterno([FromBody] JsonElement request)
        {
            _logger.LogInformation("Borrando un tipo de documento externo");

            try
            {
                string requestBody = request.GetRawText();

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_tiposDocumentoExterno", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // El SP espera la cadena JSON para extraer el ID dentro de SQL Server
                        command.Parameters.AddWithValue("@tiposDocumentoExterno", requestBody ?? (object)DBNull.Value);

                        var paramId = new SqlParameter("@idTipoDocumentoExterno", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramId);
                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        await command.ExecuteNonQueryAsync();

                        bool error = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensaje = paramMensaje.Value?.ToString() ?? string.Empty;

                        if (error)
                        {
                            return BadRequest(new { message = "Error al eliminar en base de datos", detalle = mensaje });
                        }

                        long idBorrado = paramId.Value != DBNull.Value ? Convert.ToInt64(paramId.Value) : 0;
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