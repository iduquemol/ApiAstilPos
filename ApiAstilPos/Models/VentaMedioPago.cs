#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class VentaMedioPago
    {
        public long IdMedioPagoVenta { get; set; }
        public long? IdMedioPago { get; set; }
        public decimal? ValorMedioPago { get; set; }
    }
}