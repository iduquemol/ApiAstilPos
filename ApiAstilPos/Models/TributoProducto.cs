#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class TributoProducto
    {
        public long? IdTributoProducto { get; set; }
        public string IdTributo { get; set; } = string.Empty;
        public string CodigoTributo { get; set; } = string.Empty;
        public string NombreTributo { get; set; } = string.Empty;
        public long? IdTarifaProducto { get; set; }
        public string CodigoTarifa { get; set; } = string.Empty;
        public string NombreTarifa { get; set; } = string.Empty;
        public decimal? Tarifa { get; set; }
    }
}