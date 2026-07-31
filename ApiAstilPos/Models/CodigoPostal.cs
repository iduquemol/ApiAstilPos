using Newtonsoft.Json;

namespace ApiAstilPos.Models
{
    public class CodigoPostal
    {
        public long IdCodigoPostal { get; set; }

        [JsonProperty("codigoPostal")]
        public string? Codigo { get; set; }

        public string? NombreCodigoPostal { get; set; }
        public string? TipoCodigoPostal { get; set; }
    }
}