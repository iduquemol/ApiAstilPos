#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class VentaMedioPago
    {
        public long IdMedioPagoVenta { get; set; }
        public long? IdVenta { get; set; }
        public short? IdMedioPago { get; set; }
        public decimal? ValorMedioPago { get; set; }
        public DateTime? FechaGrabacionMedioPagoVenta { get; set; }
        public long? IdTipoDocumento { get; set; }
        public long? IdTipoDocumentoExterno { get; set; }
    }
}