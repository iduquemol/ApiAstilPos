using System;

namespace ApiAstilPos.Models
{
    public class Tributo
    {
        public long IdTributo { get; set; }
        public string? CodigoTributo { get; set; }
        public string? NombreTributo { get; set; }
        public string? DescripcionTributo { get; set; }
        public long? IdCalculoTributo { get; set; }
        public long? IdTributoFe { get; set; }
        public bool? TributoProducto { get; set; }
        public bool? TributoActivo { get; set; }
        public bool? TributoRetencion { get; set; }
    }
}