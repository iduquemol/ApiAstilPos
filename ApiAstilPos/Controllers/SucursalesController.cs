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
    public class SucursalesController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SucursalesController> _logger;

        public SucursalesController(IConfiguration configuration, ILogger<SucursalesController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("sucursales")]
        public async Task<IActionResult> GetSucursales()
        {
            _logger.LogInformation("Obteniendo lista de sucursales");

            try
            {
                var sucursales = new List<Sucursal>();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_sucursalesId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonSucursales = reader.IsDBNull(reader.GetOrdinal("sucursales"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("sucursales"));

                                sucursales = JsonConvert.DeserializeObject<List<Sucursal>>(jsonSucursales);
                            }
                        }
                    }
                }

                return Ok(sucursales);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener sucursales: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("sucursales")]
        public async Task<IActionResult> CreateSucursal([FromBody] JsonElement sucursalesJson)
        {
            _logger.LogInformation("Creando una nueva sucursal");

            try
            {
                string requestBody = sucursalesJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Create_sucursales", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetro de Entrada JSON
                        command.Parameters.AddWithValue("@sucursales", requestBody ?? (object)DBNull.Value);

                        // Parámetros de Salida (OUTPUT)
                        var paramIdSucursal = new SqlParameter("@idsucursal", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramIdSucursal);
                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        // Consumir los result sets internos para que SQL Server llene los OUTPUT params
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                /* Loop de lectura para habilitar la hidratación de los parámetros OUTPUT */
                            }
                        }

                        long idSucursal = paramIdSucursal.Value != DBNull.Value ? Convert.ToInt64(paramIdSucursal.Value) : 0;
                        bool errorOutput = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeOutput = paramMensaje.Value?.ToString() ?? string.Empty;

                        _logger.LogInformation($"Procedimiento ejecutado. ID: {idSucursal}, Error: {errorOutput}");

                        if (errorOutput)
                        {
                            return BadRequest(new
                            {
                                error = true,
                                idSucursal,
                                mensaje = mensajeOutput
                            });
                        }

                        return Ok(new
                        {
                            error = false,
                            idSucursal,
                            mensaje = mensajeOutput
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear sucursal: {ex.Message}");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }

        [HttpPut("sucursales")]
        public async Task<IActionResult> UpdateSucursal([FromBody] JsonElement sucursalesJson)
        {
            _logger.LogInformation("Actualizando una sucursal");

            try
            {
                string requestBody = sucursalesJson.GetRawText();
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Update_sucursales", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Solo enviamos los 3 parámetros que define el SP:
                        command.Parameters.AddWithValue("@sucursales", requestBody ?? (object)DBNull.Value);

                        var pErrorOutput = command.Parameters.Add("@errorOutput", SqlDbType.Bit);
                        pErrorOutput.Direction = ParameterDirection.Output;

                        var pMensajeOutput = command.Parameters.Add("@mensajeOutput", SqlDbType.NVarChar, -1);
                        pMensajeOutput.Direction = ParameterDirection.Output;

                        long idSucursalEditada = 0;

                        // Consumimos el SELECT final del SP
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                idSucursalEditada = reader.IsDBNull(reader.GetOrdinal("idSucursal"))
                                    ? 0
                                    : reader.GetInt64(reader.GetOrdinal("idSucursal"));
                            }
                        }

                        bool hasError = pErrorOutput.Value != DBNull.Value && (bool)pErrorOutput.Value;
                        string mensaje = pMensajeOutput.Value?.ToString() ?? "Operación completada";

                        if (hasError)
                        {
                            return BadRequest(new { error = true, mensaje });
                        }

                        return Ok(new { message = mensaje, idSucursal = idSucursalEditada });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar sucursal: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpDelete("sucursales/{id}")]
        public async Task<IActionResult> DeleteSucursal(long id)
        {
            _logger.LogInformation($"Borrando sucursal con ID: {id}");

            try
            {
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Delete_sucursales", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // 1. Parámetro de entrada
                        command.Parameters.AddWithValue("@idSucursal", id);

                        // 2. Parámetros de salida (OUTPUT)
                        var paramError = new SqlParameter("@errorOutput", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        var paramMensaje = new SqlParameter("@mensajeOutput", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(paramError);
                        command.Parameters.Add(paramMensaje);

                        long idSucursalBorrada = 0;

                        // 3. Consumir el SELECT final del SP para poblar los OUTPUTs y obtener los valores
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                idSucursalBorrada = reader.IsDBNull(reader.GetOrdinal("idSucursal"))
                                    ? 0
                                    : reader.GetInt64(reader.GetOrdinal("idSucursal"));
                            }
                        }

                        // 4. Leer los resultados de los parámetros OUTPUT
                        bool errorOutput = paramError.Value != DBNull.Value && Convert.ToBoolean(paramError.Value);
                        string mensajeOutput = paramMensaje.Value?.ToString() ?? string.Empty;

                        _logger.LogInformation($"Procedimiento Delete ejecutado. ID: {idSucursalBorrada}, Error: {errorOutput}");

                        if (errorOutput)
                        {
                            return BadRequest(new
                            {
                                error = true,
                                idSucursal = idSucursalBorrada,
                                mensaje = mensajeOutput
                            });
                        }

                        return Ok(new
                        {
                            error = false,
                            idSucursalBorrada,
                            mensaje = mensajeOutput
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al borrar sucursal: {ex.Message}");
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}