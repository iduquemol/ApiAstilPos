#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class Moneda
    {
        public long IdMoneda { get; set; }
        public string? CodigoMoneda { get; set; }
        public string? Divisa { get; set; }
        public string? PaisAdopcion { get; set; }
        public bool? MonedaActiva { get; set; }
        public DateTime? FechaGrabacionMoneda { get; set; }
    }
}