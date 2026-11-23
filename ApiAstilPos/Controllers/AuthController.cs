using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using ApiAstilPos.Models;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString") ?? string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Intentando autenticar usuario");

            if (request == null || string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { error = true, mensaje = "Debe ingresar usuario y contraseña." });
            }

            try
            {
                string nombreUsuario = request.Usuario.Trim();
                string passUsuario = request.Password;

                short resultadoValidacion = 0;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_Valida_accesoUsuario", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@nombreUsuario", SqlDbType.VarChar, 255).Value = nombreUsuario;
                        command.Parameters.Add("@passUsuario", SqlDbType.NVarChar, 500).Value = passUsuario;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync() && !reader.IsDBNull(0))
                            {
                                resultadoValidacion = Convert.ToInt16(reader.GetValue(0));
                            }
                        }
                    }

                    if (resultadoValidacion != 1)
                    {
                        _logger.LogWarning($"Intento de login fallido para el usuario: {nombreUsuario}");
                        return Unauthorized(new { error = true, mensaje = "Usuario o contraseña incorrectos." });
                    }

                    const string queryUsuario = @"
                        SELECT TOP 1
                            idUsuario,
                            nombreUsuario,
                            idRolUsuario,
                            emailUsuario,
                            idVendedor,
                            idSucursalUsuario,
                            idAlmacen,
                            idTercero,
                            idCargoNomina,
                            enviaEventos,
                            fechaGrabacionUsuario
                        FROM [dbo].[usuarios]
                        WHERE nombreUsuario = @nombreUsuario;";

                    using (var userCommand = new SqlCommand(queryUsuario, connection))
                    {
                        userCommand.Parameters.Add("@nombreUsuario", SqlDbType.VarChar, 255).Value = nombreUsuario;

                        using (var userReader = await userCommand.ExecuteReaderAsync())
                        {
                            if (!await userReader.ReadAsync())
                            {
                                return Unauthorized(new { error = true, mensaje = "Usuario o contraseña incorrectos." });
                            }

                            var usuarioData = new
                            {
                                idUsuario = userReader["idUsuario"] != DBNull.Value ? Convert.ToInt64(userReader["idUsuario"]) : 0L,
                                nombreUsuario = userReader["nombreUsuario"]?.ToString() ?? string.Empty,
                                idRolUsuario = userReader["idRolUsuario"] != DBNull.Value ? Convert.ToInt64(userReader["idRolUsuario"]) : 0L,
                                emailUsuario = userReader["emailUsuario"]?.ToString() ?? string.Empty,
                                idVendedor = userReader["idVendedor"] != DBNull.Value ? Convert.ToInt64(userReader["idVendedor"]) : (long?)null,
                                idSucursalUsuario = userReader["idSucursalUsuario"] != DBNull.Value ? Convert.ToInt64(userReader["idSucursalUsuario"]) : (long?)null,
                                idAlmacen = userReader["idAlmacen"] != DBNull.Value ? Convert.ToInt64(userReader["idAlmacen"]) : (long?)null,
                                idTercero = userReader["idTercero"] != DBNull.Value ? Convert.ToInt64(userReader["idTercero"]) : (long?)null,
                                idCargoNomina = userReader["idCargoNomina"] != DBNull.Value ? Convert.ToInt64(userReader["idCargoNomina"]) : (long?)null,
                                enviaEventos = userReader["enviaEventos"] != DBNull.Value ? Convert.ToBoolean(userReader["enviaEventos"]) : (bool?)null,
                                fechaGrabacionUsuario = userReader["fechaGrabacionUsuario"] != DBNull.Value ? Convert.ToDateTime(userReader["fechaGrabacionUsuario"]) : (DateTime?)null
                            };

                            _logger.LogInformation($"Login exitoso para el usuario: {nombreUsuario} (idRolUsuario={usuarioData.idRolUsuario})");
                            return Ok(new
                            {
                                error = false,
                                mensaje = "Autenticación exitosa.",
                                usuario = usuarioData
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error durante el login: {ex.Message}");
                return StatusCode(500, new { error = true, mensaje = $"Error interno: {ex.Message}" });
            }
        }
    }
}