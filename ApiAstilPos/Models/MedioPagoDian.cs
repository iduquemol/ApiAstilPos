using System;

namespace ApiAstilPos.Models
{
    public class MedioPagoDian
    {
        public long IdMedioPagoDian { get; set; }

        public string CodigoMedioPagoDian { get; set; } = string.Empty;

        public string NombreMedioPagoDian { get; set; } = string.Empty;

        public string? AlcanceMedioPagoDian { get; set; }

        public bool? MedioPagoActivo { get; set; }
    }
}