using ApiAstilPos.Models;
using ApiAstilPos.Services; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QRCoder;
using System.Data;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api")]
    public class VentasController : ControllerBase
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<VentasController> _logger;

        public VentasController(EmailService emailService, IConfiguration configuration, ILogger<VentasController> logger)
        {
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("SqlConnectionString");
        }

        [HttpGet("parametrosVentaDefault")]
        public async Task<IActionResult> GetParametrosVentaDefault()
        {
            _logger.LogInformation("Obteniendo tercero de venta por defecto");

            try
            {
                var parametrosVentaDefault = new ParametrosVentaDefault();
                using var connection = new SqlConnection(GetConnectionString());
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_parametrosVentaDefault", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonTerceroVenta = reader.IsDBNull(reader.GetOrdinal("parametrosVenta")) ? "[]" : reader.GetString(reader.GetOrdinal("parametrosVenta"));
                                parametrosVentaDefault = JsonConvert.DeserializeObject<ParametrosVentaDefault>(jsonTerceroVenta);
                            }
                        }
                    }
                }
                _logger.LogInformation($"Parametros Venta Default obtenido: {parametrosVentaDefault}");
                return Ok(parametrosVentaDefault);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener parametros de venta por defecto: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("obtener-venta")]
        public async Task<IActionResult> PostVentaId([FromBody] JsonElement request)
        {
            _logger.LogInformation("Procesando solicitud para obtener venta por ID");

            try
            {
                var idVenta = request.TryGetProperty("idventa", out var ventaElement)
                    ? ventaElement.GetInt16()
                    : 0 ;

                var venta = new Venta();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Read_ventaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idVenta", idVenta);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonVenta = reader.IsDBNull(reader.GetOrdinal("venta")) ? "[]" : reader.GetString(reader.GetOrdinal("venta"));
                                venta = JsonConvert.DeserializeObject<Venta>(jsonVenta);
                            }
                        }

                        _logger.LogInformation("Venta obtenida correctamente.");
                        return Ok(venta);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener venta: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("print-venta")]
        public async Task<IActionResult> PrintVentaId([FromBody] JsonElement request)
        {
            _logger.LogInformation("Procesando solicitud para imprimir venta por ID");

            try
            {
                var idVenta = request.TryGetProperty("idventa", out var ventaElement)
                    ? ventaElement.GetInt32()
                    : 0;

                var idMetodoDian = request.TryGetProperty("idMetodoDian", out var metodoDianElement)
                  ? metodoDianElement.GetInt16()
                  : 0;

                //_logger.LogInformation("Print-Venta " + idVenta);
                var printVenta = new PrintVenta();
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Print_ventaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idVenta", idVenta);
                        command.Parameters.AddWithValue("@idMetodoDian", idMetodoDian);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonVenta = reader.IsDBNull(reader.GetOrdinal("venta")) ? "[]" : reader.GetString(reader.GetOrdinal("venta"));
                                printVenta = JsonConvert.DeserializeObject<PrintVenta>(jsonVenta);
                            }
                        }

                        _logger.LogInformation("Venta para imprimir obtenida correctamente.");
                        return Ok(printVenta);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener venta: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateFactura([FromBody] Venta venta)
        {
            _logger.LogInformation("Creando una nueva factura");

            try
            {
                string requestBody = JsonConvert.SerializeObject(venta);
                _logger.LogInformation($"Cuerpo de la solicitud: {requestBody}");

                object facturaId = null;
                object bodyDian = null;

                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    if (venta.EsBorrador)
                    {
                        using (var command = new SqlCommand("sp_Create_venta", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@venta", requestBody ?? (object)DBNull.Value);

                            facturaId = await command.ExecuteScalarAsync();

                            _logger.LogInformation("Factura creada como borrador correctamente.");
                            return Ok(new { message = "Factura creada como borrador correctamente", idFactura = facturaId });
                        }
                    }
                    else
                    {
                        using (var command = new SqlCommand("sp_Create_venta", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@venta", requestBody ?? (object)DBNull.Value);

                            facturaId = await command.ExecuteScalarAsync();
                            _logger.LogInformation("Factura creado correctamente.");
                        }

                        if (facturaId == null || Convert.ToInt64(facturaId) <= 0)
                        {
                            _logger.LogError("No se pudo obtener un ID de factura válido.");
                            return BadRequest("Error: No se pudo crear la factura.");
                        }
                        else
                        {
                            using (var command2 = new SqlCommand("sp_Read_ventaIdDian", connection))
                            {
                                command2.CommandType = CommandType.StoredProcedure;
                                command2.Parameters.AddWithValue("@idVenta", facturaId);
                                command2.Parameters.AddWithValue("@idMetodoDian", venta.IdMetodoDian);

                                bodyDian = await command2.ExecuteScalarAsync();
                                _logger.LogInformation($"Body DIAN: {bodyDian}");
                            }
                        }

                        var apiResponse = await CallExternalApiAsync(bodyDian.ToString(), facturaId, venta.IdMetodoDian);

                        if (apiResponse.IsSuccess)
                        {
                            _logger.LogInformation("API externa llamada exitosamente.");
                            using (var command = new SqlCommand("sp_Insert_responseDian", connection))
                            {
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("@responseDianJson", apiResponse.contentResponse ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@idResponseDian", 0);

                                var responseDianId = await command.ExecuteScalarAsync();
                                _logger.LogInformation("Response Factura Dian creada correctamente.");
                            }

                            // ENVIAR EMAIL
                            //try
                            //{
                            //    var facturaEmailDto = new FacturaEmailDto
                            //    {
                            //        Email = venta.TerceroVenta.EmailTercero,
                            //        NombreCliente = venta.TerceroVenta.RazonSocial,
                            //        NumeroDocumento = apiResponse.numeroFacturaDian,
                            //        Total = venta.TotalVenta
                            //    };

                            //    await _emailService.SendFacturaEmailAsync(facturaEmailDto);
                            //    _logger.LogInformation($"Email enviado exitosamente a {facturaEmailDto.Email}");
                            //}
                            //catch (Exception emailEx)
                            //{
                            //    _logger.LogWarning($"Factura creada pero falló el envío de email: {emailEx.Message}");
                            //}

                            return Ok(new
                            {
                                message = "Factura creada correctamente",
                                idFactura = facturaId,
                                numeroDocumentoDian = apiResponse.numeroFacturaDian
                            });
                        }
                        else
                        {
                            _logger.LogWarning($"Error en API externa: {apiResponse.ErrorMessage}");
                            return Ok(new
                            {
                                message = "Factura creada correctamente, pero hubo un error en la API externa",
                                idFactura = facturaId,
                                externalApiError = apiResponse.ErrorMessage
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear factura: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("preview-pdf")]
        public async Task<IActionResult> PreviewPdf([FromBody] JsonElement request)
        {
            _logger.LogInformation("Generando preview de PDF");

            try
            {
                var idVenta = request.TryGetProperty("idventa", out var ventaElement)
                    ? ventaElement.GetInt32()
                    : 0;
                var idMetodoDian = request.TryGetProperty("idMetodoDian", out var metodoDianElement)
                  ? metodoDianElement.GetInt16()
                  : 0;

                //_logger.LogInformation("Preview-pdf " + idVenta);
                PrintVenta printVenta = null;
                using (var connection = new SqlConnection(GetConnectionString()))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("sp_Print_ventaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idVenta", idVenta);
                        command.Parameters.AddWithValue("@idMetodoDian", idMetodoDian);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonVenta = reader.IsDBNull(reader.GetOrdinal("venta"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("venta"));
                                printVenta = JsonConvert.DeserializeObject<PrintVenta>(jsonVenta);  
                            }
                        }
                    }
                }

                // Generar PDF
                var pdfService = new FacturaPdfService();
                 byte[] pdfBytes = pdfService.GenerarPdfFactura(printVenta,idMetodoDian);

                // Retornar el PDF para visualización en el navegador
                return File(pdfBytes, "application/pdf", $"Preview_Factura_{idVenta}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al generar preview de PDF: {ex.Message}");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpPost("enviar-dian")]
        public async Task<IActionResult> EnviarDian([FromBody] JsonElement request)
        {
            object bodyDian = null;
            var idVenta = request.TryGetProperty("idventa", out var ventaElement)
                    ? ventaElement.GetInt32()
                    : 0;

            var idMetodoDian = request.TryGetProperty("idmetododian", out var metodoDianElement)
                    ? metodoDianElement.GetInt16()
                    : 0;

            // Declarar variables 
            long idResponseDian = 0;
            string cufe = string.Empty;
            string firmaDigital = string.Empty;
            string qrCode = string.Empty;
            byte[] attachedDocumentBytes = null;

            using (var connection = new SqlConnection(GetConnectionString()))
            {
                await connection.OpenAsync();                

                using (var command2 = new SqlCommand("sp_Read_ventaIdDian", connection))
                {
                    command2.CommandType = CommandType.StoredProcedure;
                    command2.Parameters.AddWithValue("@idVenta", idVenta);
                    command2.Parameters.AddWithValue("@idMetodoDian", idMetodoDian);

                    bodyDian = await command2.ExecuteScalarAsync();
                    _logger.LogInformation($"Body DIAN: {bodyDian}");
                }

                var apiResponse = await CallExternalApiAsync(bodyDian.ToString(), idVenta, idMetodoDian);

                if (apiResponse.IsSuccess)
                {
                    _logger.LogInformation("API externa llamada exitosamente.");

                    // Extraer el documento adjunto en base64
                    string attachedDocumentBase64 = ExtraerAttachedDocumentBase64(apiResponse.contentResponse);

                    if (!string.IsNullOrEmpty(attachedDocumentBase64))
                    {
                        // Convertir de base64 a bytes si es necesario
                        attachedDocumentBytes = Convert.FromBase64String(attachedDocumentBase64);
                        _logger.LogInformation($"Documento adjunto obtenido, tamaño: {attachedDocumentBytes.Length} bytes");

                        // Usar el documento según necesites
                    }

                    using (var command = new SqlCommand("sp_Insert_responseDian", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@responseDianJson", apiResponse.contentResponse ?? (object)DBNull.Value);
                        // Parámetros de salida
                        var paramIdResponseDian = new SqlParameter("@idResponseDian", SqlDbType.BigInt)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(paramIdResponseDian);

                        var paramCufe = new SqlParameter("@cufe", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(paramCufe);

                        var paramFirmaDigital = new SqlParameter("@firmaDigital", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(paramFirmaDigital);

                        var paramCodigoQR = new SqlParameter("@qrCode", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(paramCodigoQR);

                        await command.ExecuteNonQueryAsync();

                        // Obtener los valores de salida
                        idResponseDian = paramIdResponseDian.Value != DBNull.Value
                            ? Convert.ToInt64(paramIdResponseDian.Value)
                            : 0;
                        cufe = paramCufe.Value != DBNull.Value
                            ? paramCufe.Value.ToString()
                            : string.Empty;
                        firmaDigital = paramFirmaDigital.Value != DBNull.Value
                            ? paramFirmaDigital.Value.ToString()
                            : string.Empty;
                        qrCode = paramCodigoQR.Value != DBNull.Value
                            ? paramCodigoQR.Value.ToString()
                            : string.Empty;

                        _logger.LogInformation($"Response Factura Dian creada correctamente. ID: {idResponseDian}, CUFE: {cufe}");
                    }

                    // Obtener datos para imprimir la factura
                    PrintVenta printVenta = null;
                    using (var command = new SqlCommand("sp_Print_ventaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idVenta", idVenta);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonVenta = reader.IsDBNull(reader.GetOrdinal("venta"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("venta"));
                                printVenta = JsonConvert.DeserializeObject<PrintVenta>(jsonVenta);
                            }
                        }
                    }

                    // GENERAR PDF Y ENVIAR EMAIL
                    try
                    {
                        // Generar PDF
                        var pdfService = new FacturaPdfService();
                        printVenta.Cufe = cufe;
                        printVenta.CodigoQR = qrCode;
                        byte[] pdfBytes = pdfService.GenerarPdfFactura(printVenta, idMetodoDian);

                        _logger.LogInformation("PDF de factura generado correctamente.");

                        // Obtener XML desde la API externa usando el CUFE
                        //byte[] xmlBytes = null;
                        //if (!string.IsNullOrEmpty(cufe))
                        //{
                        //    xmlBytes = await ObtenerXmlDesdeApiAsync(cufe, printVenta.TypeEnvironmentId);

                        //    if (xmlBytes != null)
                        //    {
                        //        _logger.LogInformation("XML obtenido exitosamente desde la API externa.");
                        //    }
                        //    else
                        //    {
                        //        _logger.LogWarning("No se pudo obtener el XML desde la API externa.");
                        //    }
                        //}
                        //else
                        //{
                        //    _logger.LogWarning("CUFE no disponible, no se puede obtener el XML.");
                        //}

                        // Preparar datos del email
                        var facturaEmailDto = new FacturaEmailDto
                        {
                            Email = printVenta.ClienteEmail,
                            NombreCliente = printVenta.ClienteRazonSocial,
                            NumeroDocumento = apiResponse.numeroFacturaDian,
                            SubjectEmail = printVenta.SubjectEmail ?? string.Empty,
                            Total = printVenta.TotalVenta,
                            PdfAttachment = pdfBytes,
                            PdfFileName = $"Factura_{apiResponse.numeroFacturaDian}.pdf",
                            XmlAttachment = attachedDocumentBytes,
                            XmlFileName = $"Factura_{apiResponse.numeroFacturaDian}.xml",
                            FacturadorNombre = printVenta.FacturadorNombre ?? string.Empty,
                        };

                        // Enviar email con PDF adjunto
                        await _emailService.SendFacturaEmailAsync(facturaEmailDto, idMetodoDian);
                        _logger.LogInformation($"Email con PDF enviado exitosamente a {facturaEmailDto.Email}");

                        // Ejecutar sp_Read_ventaExterna después de enviar el email
                        var registrosNova = new List<double>();
                        using (var command = new SqlCommand("sp_Read_ventaExterna", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@idVenta", idVenta);

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var registroNova = reader.IsDBNull(reader.GetOrdinal("idRegistroExterno"))
                                        ? 0
                                        : reader.GetInt64(reader.GetOrdinal("idRegistroExterno"));

                                    if (registroNova != 0)
                                    {
                                        registrosNova.Add(registroNova);
                                        _logger.LogInformation($"Registro Nova procesado: {registroNova}");
                                    }
                                }
                            }

                            _logger.LogInformation($"sp_Read_ventaExterna ejecutado correctamente. Total registros: {registrosNova.Count}");
                        }
                        // Procesar cada registro si es necesario
                        foreach (var registro in registrosNova)
                        {
                            _logger.LogInformation($"Procesando registro Nova: {registro}");

                            // Ejecutar sp_Read_ventaExterna para obtener el JSON del registro
                            string requestRegistroNova = string.Empty;
                            using (var commandRegistro = new SqlCommand("sp_Read_RequestVentaExterna", connection))
                            {
                                commandRegistro.CommandType = CommandType.StoredProcedure;
                                commandRegistro.Parameters.AddWithValue("@idRegistro", registro);

                                using (var readerRegistro = await commandRegistro.ExecuteReaderAsync())
                                {
                                    if (await readerRegistro.ReadAsync())
                                    {
                                        requestRegistroNova = readerRegistro.IsDBNull(readerRegistro.GetOrdinal("requestRegistroNova"))
                                            ? string.Empty
                                            : readerRegistro.GetString(readerRegistro.GetOrdinal("requestRegistroNova"));
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(requestRegistroNova))
                            {
                                // Llamar al método para crear el registro en la API externa
                                var responseNova = await CrearRegistroNovaAsync(requestRegistroNova);

                                if (responseNova.IsSuccess)
                                {
                                    _logger.LogInformation($"Registro Nova creado exitosamente: {responseNova.contentResponse}");                                    
                                }
                                else
                                {
                                    _logger.LogWarning($"Error al crear registro Nova: {responseNova.ErrorMessage}");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"No se obtuvo JSON para el registro: {registro}");
                            }
                        }
                        // Ejecutar sp_Read_procesarNovasoft después de crear el registro
                        string requestProcesarNova = string.Empty;
                        using (var commandProcesar = new SqlCommand("sp_Read_procesarNovasoft", connection))
                        {
                            commandProcesar.CommandType = CommandType.StoredProcedure;
                            commandProcesar.Parameters.AddWithValue("@idVenta", idVenta);

                            using (var readerProcesar = await commandProcesar.ExecuteReaderAsync())
                            {
                                if (await readerProcesar.ReadAsync())
                                {
                                    requestProcesarNova = readerProcesar.IsDBNull(readerProcesar.GetOrdinal("procesarNova"))
                                        ? string.Empty
                                        : readerProcesar.GetString(readerProcesar.GetOrdinal("procesarNova"));

                                    _logger.LogInformation($"sp_Read_procesarNovasoft ejecutado correctamente: {requestProcesarNova}");
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(requestProcesarNova))
                        {

                            var responseProcesarNova = await ProcesarNovaAsync(requestProcesarNova);
                            if (responseProcesarNova.IsSuccess)
                            {
                                _logger.LogInformation($"Procesar en Nova ejecutado exitosamente: {responseProcesarNova.contentResponse}");
                                // Convertir el string a JSON y retornarlo
                                try
                                {
                                    var jsonResponse = JsonConvert.DeserializeObject<List<ResponseProcesarNova>>(responseProcesarNova.contentResponse);
                                    return Ok(jsonResponse);
                                }
                                catch (Newtonsoft.Json.JsonException jsonEx)
                                {
                                    _logger.LogWarning($"No se pudo deserializar el response de Nova: {jsonEx.Message}");
                                    return Ok(new
                                    {
                                        message = "Factura procesada pero hubo un problema al deserializar la respuesta de Nova",
                                        idFactura = idVenta,
                                        numeroDocumentoDian = apiResponse.numeroFacturaDian,
                                        novaResponseRaw = responseProcesarNova.contentResponse
                                    });
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Error al procesar en Nova: {responseProcesarNova.ErrorMessage}");
                            }
                        }
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning($"Factura creada pero falló el envío de email: {emailEx.Message}");
                        return Ok(new
                        {
                            message = "Factura creada pero falló el envío de email",
                            idFactura = idVenta                            
                        });
                    }

                    return Ok(new
                    {
                        message = "Factura enviada correctamente",
                        idFactura = idVenta,
                        numeroDocumentoDian = apiResponse.numeroFacturaDian
                    });
                }
                else
                {
                    _logger.LogWarning($"Error en API externa: {apiResponse.ErrorMessage}");
                    return Ok(new
                    {
                        message = "Hubo un error en la API externa",
                        idFactura = idVenta,
                        idMetodoDian = idMetodoDian,
                        externalApiError = apiResponse.ErrorMessage
                    });
                }
            }
        }

        private async Task<ApiResponse> CallExternalApiAsync(string originalRequestBody, object facturaId, long? IdMetodoDian)
        {
            try
            {
                ResponseDian responseDian = new ResponseDian();
                string apiUrl = String.Empty;
                if (IdMetodoDian == 1)
                {
                    apiUrl = _configuration["ApiExterna:InvoiceUrl"];
                }
                else if (IdMetodoDian == 2)
                {
                    apiUrl = _configuration["ApiExterna:PosUrl"];
                }
                else if (IdMetodoDian == 3)
                {
                    apiUrl = _configuration["ApiExterna:NotaCUrl"];
                }

                var bearerToken = _configuration["ApiExterna:BearerToken"];

                if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(bearerToken))
                {
                    _logger.LogError("URL de API externa o Bearer Token no configurados");
                    return new ApiResponse { IsSuccess = false, ErrorMessage = "Configuración de API externa faltante" };
                }

                var content = new StringContent(originalRequestBody, Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

                _logger.LogInformation($"Llamando a API externa: {apiUrl}");

                var response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"API externa respondió exitosamente: {response.StatusCode}");
                    responseDian = JsonConvert.DeserializeObject<ResponseDian>(responseContent);

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        numeroFacturaDian = responseDian.Number,
                        contentResponse = responseContent,
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error en API externa: {response.StatusCode} - {errorContent}");

                    return new ApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"API externa falló: {response.StatusCode} - {errorContent} - {apiUrl}",

                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"Error de conexión con API externa: {httpEx.Message}");
                return new ApiResponse { IsSuccess = false, ErrorMessage = $"Error de conexión: {httpEx.Message}" };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado al llamar API externa: {ex.Message}");
                return new ApiResponse { IsSuccess = false, ErrorMessage = $"Error inesperado: {ex.Message}" };
            }
        }

        private async Task<byte[]> ObtenerXmlDesdeApiAsync(string cufe, int typeEnvironmentId)
        {
            try
            {
                if (string.IsNullOrEmpty(cufe))
                {
                    _logger.LogWarning("CUFE vacío, no se puede obtener XML");
                    return null;
                }

                var xmlApiUrl = _configuration["ApiExterna:XmlUrl"];
                var bearerToken = _configuration["ApiExterna:BearerToken"];

                if (string.IsNullOrEmpty(xmlApiUrl))
                {
                    _logger.LogError("URL de API de XML no configurada");
                    return null;
                }

                // Construir URL con query string (CUFE)
                var urlConCufe = $"{xmlApiUrl}/{Uri.EscapeDataString(cufe)}";

                // Crear el body del request
                var requestBody = new XmlApiRequest
                {
                    environment = new XmlEnvironment
                    {
                        type_environment_id = typeEnvironmentId
                    }
                };

                var jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

                _logger.LogInformation($"Llamando a API de XML: {urlConCufe}");
                _logger.LogInformation($"Body request: {jsonContent}");

                var response = await httpClient.PostAsync(urlConCufe, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"API de XML respondió exitosamente: {response.StatusCode}");

                    var xmlResponse = JsonConvert.DeserializeObject<XmlApiResponse>(responseContent);

                    if (!string.IsNullOrEmpty(xmlResponse?.XmlBytesBase64))
                    {
                        // Convertir de base64 a bytes
                        byte[] xmlBytes = Convert.FromBase64String(xmlResponse.XmlBytesBase64);
                        _logger.LogInformation($"XML obtenido correctamente, tamaño: {xmlBytes.Length} bytes");
                        return xmlBytes;
                    }
                    else
                    {
                        _logger.LogWarning("La respuesta de la API no contiene XmlBytesBase64");
                        return null;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error en API de XML: {response.StatusCode} - {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener XML desde API: {ex.Message}");
                return null;
            }
        }

        private async Task<ApiResponse> CrearRegistroNovaAsync(string requestBody)
        {
            try
            {
                var novaApiUrl = _configuration["ApiExterna:NovaUrl"];
                //var bearerToken = _configuration["ApiExterna:BearerToken"];

                if (string.IsNullOrEmpty(novaApiUrl))
                {
                    _logger.LogError("URL de API Nova o Bearer Token no configurados");
                    return new ApiResponse { IsSuccess = false, ErrorMessage = "Configuración de API Nova faltante" };
                }

                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Clear();
                //httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

                _logger.LogInformation($"Llamando a API Nova: {novaApiUrl}");
                _logger.LogInformation($"Body request: {requestBody}");

                var response = await httpClient.PostAsync(novaApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"API Nova respondió exitosamente: {response.StatusCode}");

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        contentResponse = responseContent
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error en API Nova: {response.StatusCode} - {errorContent}");

                    return new ApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"API Nova falló: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"Error de conexión con API Nova: {httpEx.Message}");
                return new ApiResponse { IsSuccess = false, ErrorMessage = $"Error de conexión: {httpEx.Message}" };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado al llamar API Nova: {ex.Message}");
                return new ApiResponse { IsSuccess = false, ErrorMessage = $"Error inesperado: {ex.Message}" };
            }
        }

        private async Task<ApiResponse> ProcesarNovaAsync(string requestBody)
        {
            try
            {
                var novaApiUrl = _configuration["ApiExterna:NovaProcesarUrl"];
                //var bearerToken = _configuration["ApiExterna:BearerToken"];

                if (string.IsNullOrEmpty(novaApiUrl))
                {
                    _logger.LogError("URL de API Nova o Bearer Token no configurados");
                    return new ApiResponse { IsSuccess = false, ErrorMessage = "Configuración de API Nova faltante" };
                }

                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                httpClient.DefaultRequestHeaders.Clear();
                //httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

                _logger.LogInformation($"Llamando a API Nova: {novaApiUrl}");
                _logger.LogInformation($"Body request: {requestBody}");

                var response = await httpClient.PostAsync(novaApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"API Nova respondió exitosamente: {response.StatusCode}");

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        contentResponse = responseContent
                    };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error en API Nova: {response.StatusCode} - {errorContent}");

                    return new ApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = $"API Nova falló: {response.StatusCode} - {errorContent}"
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"Error de conexión con API Nova: {httpEx.Message}");
                return new ApiResponse { IsSuccess = false, ErrorMessage = $"Error de conexión: {httpEx.Message}" };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado al llamar API Nova: {ex.Message}");
                return new ApiResponse { IsSuccess = false, ErrorMessage = $"Error inesperado: {ex.Message}" };
            }
        }

        private string ExtraerAttachedDocumentBase64(string xmlContent)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlContent))
                {
                    _logger.LogWarning("Contenido XML vacío para extraer attached_document_base64_bytes");
                    return string.Empty;
                }

                // Parsear el JSON que contiene el XML
                var jsonResponse = JObject.Parse(xmlContent);

                // Intentar obtener attached_document_base64_bytes directamente si está en el JSON
                var attachedDocumentBase64 = jsonResponse["attached_document_base64_bytes"]?.ToString();

                if (!string.IsNullOrEmpty(attachedDocumentBase64))
                {
                    _logger.LogInformation("attached_document_base64_bytes extraído exitosamente del JSON");
                    return attachedDocumentBase64;
                }

                // Si no está en el JSON directamente, buscar en el XML si existe
                var xmlString = jsonResponse["xml"]?.ToString() ??
                               jsonResponse["AttachedDocument"]?.ToString() ??
                               xmlContent;

                if (xmlString.Contains("attached_document_base64_bytes"))
                {
                    // Usar expresión regular para extraer el valor
                    var match = System.Text.RegularExpressions.Regex.Match(
                        xmlString,
                        @"<attached_document_base64_bytes>(.*?)</attached_document_base64_bytes>",
                        System.Text.RegularExpressions.RegexOptions.Singleline
                    );

                    if (match.Success && match.Groups.Count > 1)
                    {
                        attachedDocumentBase64 = match.Groups[1].Value.Trim();
                        _logger.LogInformation($"attached_document_base64_bytes extraído del XML, tamaño: {attachedDocumentBase64.Length} caracteres");
                        return attachedDocumentBase64;
                    }
                }

                _logger.LogWarning("No se encontró attached_document_base64_bytes en el contenido");
                return string.Empty;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError($"Error al parsear JSON para extraer attached_document_base64_bytes: {jsonEx.Message}");

                // Intentar como XML puro si falla el JSON
                try
                {
                    var match = System.Text.RegularExpressions.Regex.Match(
                        xmlContent,
                        @"<attached_document_base64_bytes>(.*?)</attached_document_base64_bytes>",
                        System.Text.RegularExpressions.RegexOptions.Singleline
                    );

                    if (match.Success && match.Groups.Count > 1)
                    {
                        var result = match.Groups[1].Value.Trim();
                        _logger.LogInformation($"attached_document_base64_bytes extraído del XML puro, tamaño: {result.Length} caracteres");
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al procesar XML: {ex.Message}");
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado al extraer attached_document_base64_bytes: {ex.Message}");
                return string.Empty;
            }
        }

        [HttpPost("obtener-zip")]
        public async Task<IActionResult> ObtenerZip([FromBody] JsonElement request)
        {
            object idVenta = null;
            var num_doc = request.TryGetProperty("num_doc", out var numDoc)
                   ? numDoc.ToString()
                   : "0";

            using (var connection = new SqlConnection(GetConnectionString()))
            {
                await connection.OpenAsync();

                using (var command2 = new SqlCommand("sp_Response_ventaExterna", connection))
                {
                    command2.CommandType = CommandType.StoredProcedure;
                    command2.Parameters.AddWithValue("@num_doc", num_doc);

                    idVenta = await command2.ExecuteScalarAsync();
                }

                if (idVenta == null || Convert.ToInt64(idVenta) <= 0)
                {
                    _logger.LogError("No se pudo obtener un ID de venta válido.");
                    return BadRequest("Error: No se encontró la venta.");
                }

                PrintVenta printVenta = null;
                using (var command = new SqlCommand("sp_Print_ventaId", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@idVenta", idVenta);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var jsonVenta = reader.IsDBNull(reader.GetOrdinal("venta"))
                                ? "[]"
                                : reader.GetString(reader.GetOrdinal("venta"));
                            printVenta = JsonConvert.DeserializeObject<PrintVenta>(jsonVenta);
                        }
                    }
                }

                try
                {
                    var pdfService = new FacturaPdfService();
                    byte[] pdfBytes = pdfService.GenerarPdfFactura(printVenta, 1);

                    _logger.LogInformation("PDF de factura generado correctamente.");

                    // Obtener XML desde la API externa usando el CUFE
                    byte[] attachedDocumentBytes = null;
                    attachedDocumentBytes = Convert.FromBase64String(printVenta.xmlBase64);

                    //if (!string.IsNullOrEmpty(printVenta.Cufe))
                    //{
                    //    xmlBytes = await ObtenerXmlDesdeApiAsync(printVenta.Cufe, printVenta.TypeEnvironmentId);
                    //    if (xmlBytes != null)
                    //    {
                    //        _logger.LogInformation("XML obtenido exitosamente desde la API externa.");
                    //    }
                    //    else
                    //    {
                    //        _logger.LogWarning("No se pudo obtener el XML desde la API externa.");
                    //    }
                    //}
                    //else
                    //{
                    //    _logger.LogWarning("CUFE no disponible, no se puede obtener el XML.");
                    //}

                    // Crear archivo ZIP
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                        {
                            // Agregar PDF al ZIP
                            var pdfEntry = archive.CreateEntry($"Factura_{printVenta.PrefijoVenta}{printVenta.NumeroVenta}.pdf");
                            using (var entryStream = pdfEntry.Open())
                            {
                                await entryStream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                            }

                            // Agregar XML al ZIP si existe
                            if (attachedDocumentBytes != null && attachedDocumentBytes.Length > 0)
                            {
                                var xmlEntry = archive.CreateEntry($"Factura_{printVenta.PrefijoVenta}{printVenta.NumeroVenta}.xml");
                                using (var entryStream = xmlEntry.Open())
                                {
                                    await entryStream.WriteAsync(attachedDocumentBytes, 0, attachedDocumentBytes.Length);
                                }
                            }
                        }

                        memoryStream.Position = 0;
                        var zipBytes = memoryStream.ToArray();

                        _logger.LogInformation($"ZIP creado correctamente con tamaño: {zipBytes.Length} bytes");

                        // Convertir el ZIP a Base64
                        string zipBase64 = Convert.ToBase64String(zipBytes);

                        // Retornar el ZIP en Base64
                        return Ok(new
                        {
                            message = "ZIP generado correctamente",
                            fileName = $"Factura_{printVenta.PrefijoVenta}{printVenta.NumeroVenta}.zip",
                            zipBase64 = zipBase64,
                            size = zipBytes.Length
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al generar PDF o obtener XML: {ex.Message}");
                    return BadRequest($"Error: {ex.Message}");
                }
            }
                
        }

        [HttpPost("enviar-nota-dian")]
        public async Task<IActionResult> EnviarNotaDian([FromBody] JsonElement request)
        {
            object bodyDian = null;
            var idVenta = request.TryGetProperty("idventa", out var ventaElement)
                    ? ventaElement.GetInt32()
                    : 0;

            var idMetodoDian = request.TryGetProperty("idmetododian", out var metodoDianElement)
                    ? metodoDianElement.GetInt16()
                    : 0;

            // Declarar variables 
            long idResponseDian = 0;
            string cufe = string.Empty;
            string firmaDigital = string.Empty;
            string qrCode = string.Empty;
            byte[] attachedDocumentBytes = null;

            using (var connection = new SqlConnection(GetConnectionString()))
            {
                await connection.OpenAsync();

                using (var command2 = new SqlCommand("sp_Read_ventaIdDian", connection))
                {
                    command2.CommandType = CommandType.StoredProcedure;
                    command2.Parameters.AddWithValue("@idVenta", idVenta);
                    command2.Parameters.AddWithValue("@idMetodoDian", idMetodoDian);

                    bodyDian = await command2.ExecuteScalarAsync();
                    _logger.LogInformation($"Body DIAN: {bodyDian}");
                }

                var apiResponse = await CallExternalApiAsync(bodyDian.ToString(), idVenta, idMetodoDian);

                if (apiResponse.IsSuccess)
                {
                    _logger.LogInformation("API externa llamada exitosamente.");

                    // Extraer el documento adjunto en base64
                    string attachedDocumentBase64 = ExtraerAttachedDocumentBase64(apiResponse.contentResponse);

                    if (!string.IsNullOrEmpty(attachedDocumentBase64))
                    {
                        // Convertir de base64 a bytes si es necesario
                        attachedDocumentBytes = Convert.FromBase64String(attachedDocumentBase64);
                        _logger.LogInformation($"Documento adjunto obtenido, tamaño: {attachedDocumentBytes.Length} bytes");

                        // Usar el documento según necesites
                    }                    

                    using (var command = new SqlCommand("sp_Insert_responseDian", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@responseDianJson", apiResponse.contentResponse ?? (object)DBNull.Value);
                        // Parámetros de salida
                        var paramIdResponseDian = new SqlParameter("@idResponseDian", SqlDbType.BigInt)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(paramIdResponseDian);

                        var paramCufe = new SqlParameter("@cufe", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(paramCufe);

                        var paramFirmaDigital = new SqlParameter("@firmaDigital", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(paramFirmaDigital);

                        var paramCodigoQR = new SqlParameter("@qrCode", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(paramCodigoQR);

                        await command.ExecuteNonQueryAsync();

                        // Obtener los valores de salida
                        idResponseDian = paramIdResponseDian.Value != DBNull.Value
                            ? Convert.ToInt64(paramIdResponseDian.Value)
                            : 0;
                        cufe = paramCufe.Value != DBNull.Value
                            ? paramCufe.Value.ToString()
                            : string.Empty;
                        firmaDigital = paramFirmaDigital.Value != DBNull.Value
                            ? paramFirmaDigital.Value.ToString()
                            : string.Empty;
                        qrCode = paramCodigoQR.Value != DBNull.Value
                            ? paramCodigoQR.Value.ToString()
                            : string.Empty;

                        _logger.LogInformation($"Response Factura Dian creada correctamente. ID: {idResponseDian}, CUFE: {cufe}");
                    }

                    // Obtener datos para imprimir la factura
                    PrintVenta printVenta = null;
                    using (var command = new SqlCommand("sp_Print_ventaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idVenta", idVenta);
                        command.Parameters.AddWithValue("@idMetodoDian", idMetodoDian);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonVenta = reader.IsDBNull(reader.GetOrdinal("venta"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("venta"));
                                printVenta = JsonConvert.DeserializeObject<PrintVenta>(jsonVenta);
                            }
                        }
                    }

                    // GENERAR PDF Y ENVIAR EMAIL
                    try
                    {
                        // Generar PDF
                        var pdfService = new FacturaPdfService();
                        printVenta.Cufe = cufe;
                        printVenta.CodigoQR = qrCode;
                        byte[] pdfBytes = pdfService.GenerarPdfFactura(printVenta, idMetodoDian);

                        _logger.LogInformation("PDF de factura generado correctamente.");                       

                        // Preparar datos del email
                        var facturaEmailDto = new FacturaEmailDto
                        {
                            Email = printVenta.ClienteEmail,
                            NombreCliente = printVenta.ClienteRazonSocial,
                            NumeroDocumento = apiResponse.numeroFacturaDian,
                            SubjectEmail = printVenta.SubjectEmail ?? string.Empty,
                            Total = printVenta.TotalVenta,
                            PdfAttachment = pdfBytes,
                            PdfFileName = $"NotaC_{apiResponse.numeroFacturaDian}.pdf",
                            XmlAttachment = attachedDocumentBytes,
                            XmlFileName = $"NotaC_{apiResponse.numeroFacturaDian}.xml",
                            FacturadorNombre = printVenta.FacturadorNombre ?? string.Empty,
                        };

                        // Enviar email con PDF adjunto
                        await _emailService.SendFacturaEmailAsync(facturaEmailDto, idMetodoDian);
                        _logger.LogInformation($"Email con PDF enviado exitosamente a {facturaEmailDto.Email}");

                        // Ejecutar sp_Read_ventaExterna después de enviar el email
                        var registrosNova = new List<double>();
                        using (var command = new SqlCommand("sp_Read_ventaExterna", connection))
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.AddWithValue("@idVenta", idVenta);

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var registroNova = reader.IsDBNull(reader.GetOrdinal("idRegistroExterno"))
                                        ? 0
                                        : reader.GetInt64(reader.GetOrdinal("idRegistroExterno"));

                                    if (registroNova != 0)
                                    {
                                        registrosNova.Add(registroNova);
                                        _logger.LogInformation($"Registro Nova procesado: {registroNova}");
                                    }
                                }
                            }

                            _logger.LogInformation($"sp_Read_ventaExterna ejecutado correctamente. Total registros: {registrosNova.Count}");
                        }
                        // Procesar cada registro si es necesario
                        foreach (var registro in registrosNova)
                        {
                            _logger.LogInformation($"Procesando registro Nova: {registro}");

                            // Ejecutar sp_Read_ventaExterna para obtener el JSON del registro
                            string requestRegistroNova = string.Empty;
                            using (var commandRegistro = new SqlCommand("sp_Read_RequestVentaExterna", connection))
                            {
                                commandRegistro.CommandType = CommandType.StoredProcedure;
                                commandRegistro.Parameters.AddWithValue("@idRegistro", registro);

                                using (var readerRegistro = await commandRegistro.ExecuteReaderAsync())
                                {
                                    if (await readerRegistro.ReadAsync())
                                    {
                                        requestRegistroNova = readerRegistro.IsDBNull(readerRegistro.GetOrdinal("requestRegistroNova"))
                                            ? string.Empty
                                            : readerRegistro.GetString(readerRegistro.GetOrdinal("requestRegistroNova"));
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(requestRegistroNova))
                            {
                                // Llamar al método para crear el registro en la API externa
                                var responseNova = await CrearRegistroNovaAsync(requestRegistroNova);

                                if (responseNova.IsSuccess)
                                {
                                    _logger.LogInformation($"Registro Nova creado exitosamente: {responseNova.contentResponse}");
                                }
                                else
                                {
                                    _logger.LogWarning($"Error al crear registro Nova: {responseNova.ErrorMessage}");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"No se obtuvo JSON para el registro: {registro}");
                            }
                        }
                        // Ejecutar sp_Read_procesarNovasoft después de crear el registro
                        string requestProcesarNova = string.Empty;
                        using (var commandProcesar = new SqlCommand("sp_Read_procesarNovasoft", connection))
                        {
                            commandProcesar.CommandType = CommandType.StoredProcedure;
                            commandProcesar.Parameters.AddWithValue("@idVenta", idVenta);

                            using (var readerProcesar = await commandProcesar.ExecuteReaderAsync())
                            {
                                if (await readerProcesar.ReadAsync())
                                {
                                    requestProcesarNova = readerProcesar.IsDBNull(readerProcesar.GetOrdinal("procesarNova"))
                                        ? string.Empty
                                        : readerProcesar.GetString(readerProcesar.GetOrdinal("procesarNova"));

                                    _logger.LogInformation($"sp_Read_procesarNovasoft ejecutado correctamente: {requestProcesarNova}");
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(requestProcesarNova))
                        {

                            var responseProcesarNova = await ProcesarNovaAsync(requestProcesarNova);
                            if (responseProcesarNova.IsSuccess)
                            {
                                _logger.LogInformation($"Procesar en Nova ejecutado exitosamente: {responseProcesarNova.contentResponse}");
                                // Convertir el string a JSON y retornarlo
                                try
                                {
                                    var jsonResponse = JsonConvert.DeserializeObject<object>(responseProcesarNova.contentResponse);
                                    return Ok(jsonResponse);
                                }
                                catch (Newtonsoft.Json.JsonException jsonEx)
                                {
                                    _logger.LogWarning($"No se pudo deserializar el response de Nova: {jsonEx.Message}");
                                    return Ok(new
                                    {
                                        message = "Factura procesada pero hubo un problema al deserializar la respuesta de Nova",
                                        idFactura = idVenta,
                                        numeroDocumentoDian = apiResponse.numeroFacturaDian,
                                        novaResponseRaw = responseProcesarNova.contentResponse
                                    });
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Error al procesar en Nova: {responseProcesarNova.ErrorMessage}");
                            }
                        }
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogWarning($"Factura creada pero falló el envío de email: {emailEx.Message}");
                        return Ok(new
                        {
                            message = "Factura creada pero falló el envío de email",
                            idFactura = idVenta
                        });
                    }

                    return Ok(new
                    {
                        message = "Factura enviada correctamente",
                        idFactura = idVenta,
                        numeroDocumentoDian = apiResponse.numeroFacturaDian
                    });
                }
                else
                {
                    _logger.LogWarning($"Error en API externa: {apiResponse.ErrorMessage}");
                    return Ok(new
                    {
                        message = "Hubo un error en la API externa",
                        idFactura = idVenta,
                        idMetodoDian = idMetodoDian,
                        externalApiError = apiResponse.ErrorMessage
                    });
                }
            }
        }

        [HttpPost("obtener-xml")]
        public async Task<IActionResult> ObtenerXml([FromBody] JsonElement request)
        {
            object idVenta = null;
            string attachedDocument = null;
            var num_doc = request.TryGetProperty("num_doc", out var numDoc)
                   ? numDoc.ToString()
                   : "0";

            using (var connection = new SqlConnection(GetConnectionString()))
            {
                await connection.OpenAsync();

                using (var command2 = new SqlCommand("sp_Response_ventaExterna", connection))
                {
                    command2.CommandType = CommandType.StoredProcedure;
                    command2.Parameters.AddWithValue("@num_doc", num_doc);

                    using (var reader = await command2.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            idVenta = reader.IsDBNull(reader.GetOrdinal("idVenta"))
                                ? null
                                : reader.GetInt64(reader.GetOrdinal("idVenta"));

                            attachedDocument = reader.IsDBNull(reader.GetOrdinal("attachedDocument"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("attachedDocument"));
                        }
                    }
                }

                if (idVenta == null || Convert.ToInt64(idVenta) <= 0)
                {
                    _logger.LogError("No se pudo obtener un ID de venta válido.");
                    return BadRequest("Error: No se encontró la venta.");
                }

                if (string.IsNullOrEmpty(attachedDocument))
                {
                    _logger.LogError("No se pudo obtener el documento XML adjunto.");
                    return BadRequest("Error: No se encontró el documento XML.");
                }

                try
                {
                    // Convertir de base64 a bytes
                    byte[] xmlBytes = Convert.FromBase64String(attachedDocument);
                    _logger.LogInformation($"XML obtenido correctamente, tamaño: {xmlBytes.Length} bytes");                   

                    // Retornar el XML como archivo descargable
                    return File(xmlBytes, "application/xml");
                }
                catch (FormatException ex)
                {
                    _logger.LogError($"Error al decodificar base64: {ex.Message}");
                    return BadRequest("Error: El documento adjunto no tiene un formato base64 válido.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al generar archivo XML: {ex.Message}");
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }

        [HttpPost("enviar-correo")]
        public async Task<IActionResult> EnviarCorreo([FromBody] JsonElement request)
        {
            object idVenta = null;
            string attachedDocument = null;
            var num_doc = request.TryGetProperty("num_doc", out var numDoc)
                   ? numDoc.ToString()
                   : "0";
            var idMetodoDian = request.TryGetProperty("idMetodoDian", out var metodoDianElement)
                   ? metodoDianElement.GetInt16()
                   : 0;

            using (var connection = new SqlConnection(GetConnectionString()))
            {
                await connection.OpenAsync();

                using (var command2 = new SqlCommand("sp_Response_ventaExterna", connection))
                {
                    command2.CommandType = CommandType.StoredProcedure;
                    command2.Parameters.AddWithValue("@num_doc", num_doc);

                    using (var reader = await command2.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            idVenta = reader.IsDBNull(reader.GetOrdinal("idVenta"))
                                ? null
                                : reader.GetInt64(reader.GetOrdinal("idVenta"));

                            attachedDocument = reader.IsDBNull(reader.GetOrdinal("attachedDocument"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("attachedDocument"));
                        }
                    }
                }

                if (idVenta == null || Convert.ToInt64(idVenta) <= 0)
                {
                    _logger.LogError("No se pudo obtener un ID de venta válido.");
                    return BadRequest("Error: No se encontró la venta.");
                }

                if (string.IsNullOrEmpty(attachedDocument))
                {
                    _logger.LogError("No se pudo obtener el documento XML adjunto.");
                    return BadRequest("Error: No se encontró el documento XML.");
                }

                try
                {
                    // Convertir de base64 a bytes
                    byte[] xmlBytes = Convert.FromBase64String(attachedDocument);
                    _logger.LogInformation($"XML obtenido correctamente, tamaño: {xmlBytes.Length} bytes");

                    // Obtener datos para imprimir la factura
                    PrintVenta printVenta = null;
                    using (var command = new SqlCommand("sp_Print_ventaId", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@idVenta", idVenta);
                        command.Parameters.AddWithValue("@idMetodoDian", idMetodoDian);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var jsonVenta = reader.IsDBNull(reader.GetOrdinal("venta"))
                                    ? "[]"
                                    : reader.GetString(reader.GetOrdinal("venta"));
                                printVenta = JsonConvert.DeserializeObject<PrintVenta>(jsonVenta);
                            }
                        }
                    }

                    // Generar PDF
                    var pdfService = new FacturaPdfService();
                    byte[] pdfBytes = pdfService.GenerarPdfFactura(printVenta, idMetodoDian);

                    _logger.LogInformation("PDF de factura generado correctamente.");

                    // Preparar datos del email
                    var facturaEmailDto = new FacturaEmailDto
                    {
                        Email = printVenta.ClienteEmail,
                        NombreCliente = printVenta.ClienteRazonSocial,
                        NumeroDocumento = printVenta.NumeroVenta.ToString(),
                        SubjectEmail = printVenta.SubjectEmail ?? string.Empty,
                        Total = printVenta.TotalVenta,
                        PdfAttachment = pdfBytes,
                        PdfFileName = idMetodoDian == 1 ? $"Factura_{printVenta.NumeroVenta}.pdf" : $"NotaCredito_{printVenta.NumeroVenta}.pdf",
                        XmlAttachment = xmlBytes,
                        XmlFileName = idMetodoDian == 1 ? $"Factura_{printVenta.NumeroVenta}.xml" : $"NotaCredito_{printVenta.NumeroVenta}.xml",
                        FacturadorNombre = printVenta.FacturadorNombre ?? string.Empty,
                    };

                    // Enviar email con PDF adjunto
                    await _emailService.SendFacturaEmailAsync(facturaEmailDto, idMetodoDian);
                    _logger.LogInformation($"Email con PDF enviado exitosamente a {facturaEmailDto.Email}");
                    return Ok(new
                    {
                        message = "Email enviado correctamente"                        
                    });
                }
                catch (FormatException ex)
                {
                    _logger.LogError($"Error al decodificar base64: {ex.Message}");
                    return BadRequest("Error: El documento adjunto no tiene un formato base64 válido.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al generar archivo XML: {ex.Message}");
                    return BadRequest($"Error: {ex.Message}");
                }
            }
        }
    }

    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string numeroFacturaDian { get; set; }
        public string contentResponse { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ResponseProcesarNova
    {
        [JsonProperty("mensaje")]
        public string mensaje { get; set; }

        [JsonProperty("ano_doc")]
        public string ano_doc { get; set; }

        [JsonProperty("per_doc")]
        public string per_doc { get; set; }

        [JsonProperty("sub_tip")]
        public string sub_tip { get; set; }

        [JsonProperty("num_doc")]
        public string num_doc { get; set; }

        [JsonProperty("contab")]
        public int contab { get; set; }
    }
}