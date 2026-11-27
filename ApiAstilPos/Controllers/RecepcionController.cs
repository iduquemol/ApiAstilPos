using ApiAstilPos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiAstilPos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecepcionController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RecepcionController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet("obtener-recepciones")]
        public async Task<ActionResult<List<RecepcionDocumento>>> ObtenerRecepciones()
        {
            var receptionsUrl = ObtenerReceptionsUrl();
            if (string.IsNullOrWhiteSpace(receptionsUrl))
                return StatusCode(500, new { mensaje = "No está configurada la URL de recepciones en appsettings." });

            var client = CrearClienteAutorizado();

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(receptionsUrl, content: null);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new
                {
                    mensaje = "No fue posible conectarse con la API externa de recepciones.",
                    url = receptionsUrl,
                    detalle = ex.Message
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorDetalle = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new
                {
                    mensaje = "Error al consultar las recepciones en la API externa.",
                    detalle = errorDetalle
                });
            }

            var json = await response.Content.ReadAsStringAsync();
            var documentos = DeserializarLista<RecepcionDocumento>(json);

            return Ok(documentos);
        }

        [HttpGet("{uuid}/pdf")]
        public async Task<IActionResult> ObtenerPdfPorUuid(string uuid)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                return BadRequest(new { mensaje = "El UUID es obligatorio." });

            var receptionsUrl = ObtenerReceptionsUrl();
            if (string.IsNullOrWhiteSpace(receptionsUrl))
                return StatusCode(500, new { mensaje = "No está configurada la URL de recepciones en appsettings." });

            var client = CrearClienteAutorizado();
            var baseTrimmed = receptionsUrl.TrimEnd('/');
            var urlDetalle = $"{baseTrimmed}/{Uri.EscapeDataString(uuid.Trim())}";

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(urlDetalle, content: null);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, new
                {
                    mensaje = "No fue posible conectarse con la API externa para consultar el detalle.",
                    url = urlDetalle,
                    detalle = ex.Message
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorDetalle = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new
                {
                    mensaje = "Error al consultar el detalle de la recepción en la API externa.",
                    detalle = errorDetalle
                });
            }

            var json = await response.Content.ReadAsStringAsync();
            var detalles = DeserializarLista<RecepcionDetalle>(json);
            var detalle = detalles.FirstOrDefault();

            if (detalle?.Attachments is null || detalle.Attachments.Count == 0)
                return NotFound(new { mensaje = "El documento no tiene adjuntos disponibles." });

            var adjuntoPdf = detalle.Attachments.FirstOrDefault(item =>
                !string.IsNullOrWhiteSpace(item.Key) &&
                item.Key.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(adjuntoPdf.Key))
            {
                adjuntoPdf = detalle.Attachments.FirstOrDefault(item =>
                    item.Value.ValueKind != JsonValueKind.Null &&
                    item.Value.ValueKind != JsonValueKind.Undefined);
            }

            var base64Raw = ExtraerTextoBase64(adjuntoPdf.Value);
            if (string.IsNullOrWhiteSpace(base64Raw))
                return NotFound(new { mensaje = "No se encontró contenido Base64 para el PDF." });

            try
            {
                var base64Limpio = base64Raw.Contains(',')
                    ? base64Raw.Split(',')[^1]
                    : base64Raw.Trim();

                byte[] pdfBytes = Convert.FromBase64String(base64Limpio);

                var nombreArchivo = string.IsNullOrWhiteSpace(adjuntoPdf.Key)
                    ? $"{uuid}.pdf"
                    : adjuntoPdf.Key.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                        ? adjuntoPdf.Key
                        : $"{adjuntoPdf.Key}.pdf";

                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (FormatException)
            {
                return BadRequest(new { mensaje = "El contenido del adjunto no tiene un formato Base64 válido." });
            }
        }

        private static string? ExtraerTextoBase64(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
                return element.GetString();

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var texto = ExtraerTextoBase64(item);
                    if (!string.IsNullOrWhiteSpace(texto))
                        return texto;
                }
            }
            else if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in element.EnumerateObject())
                {
                    var texto = ExtraerTextoBase64(prop.Value);
                    if (!string.IsNullOrWhiteSpace(texto))
                        return texto;
                }
            }

            return null;
        }

        private static List<T> DeserializarLista<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<T>();

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                doc.RootElement.TryGetProperty("data", out var dataElement) &&
                dataElement.ValueKind == JsonValueKind.Array)
            {
                return dataElement.Deserialize<List<T>>(JsonOptions) ?? new List<T>();
            }

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new List<T>();
            }

            return new List<T>();
        }

        private string? ObtenerReceptionsUrl()
        {
            var urlDirecta = _configuration["ApiExterna:ReceptionsUrl"];
            if (!string.IsNullOrWhiteSpace(urlDirecta))
                return urlDirecta.Trim();

            return null;
        }

        private HttpClient CrearClienteAutorizado()
        {
            var client = _httpClientFactory.CreateClient();

            var token = _configuration["ApiExterna:BearerToken"];
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
            }

            return client;
        }
    }
}