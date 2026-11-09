#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class FrecuenciaPagoNomina
    {
        public long IdFrecuenciaPago { get; set; }
        public string? NombreFrecuenciaPago { get; set; } = string.Empty;
        public byte? DiasFrecuenciaPago { get; set; }
        public bool? DiasComerciales { get; set; }
        public long? IdFrecuenciaPagoDian { get; set; }
        public bool? FrecuenciaActiva { get; set; }
        public DateTime? FechaGrabacionFrecuencia { get; set; }
    }
}