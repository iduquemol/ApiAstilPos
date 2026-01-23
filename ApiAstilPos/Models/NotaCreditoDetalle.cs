using System;

namespace ApiAstilPos.Models
{
    public class NotaCreditoDetalle
    {
        public long IdDetalleNotaCredito { get; set; }
        public long RegistroNotaCredito { get; set; }
        public long IdDetalleVenta { get; set; }
        public long IdProducto { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }
        public decimal CantidadNotaCredito { get; set; }
        public decimal PrecioUnitarioNotaCredito { get; set; }
        public decimal PorcentajeIvaNotaCredito { get; set; }
        public decimal IvaNotaCredito { get; set; }
        public decimal PorcentajeDescuentoNotaCredito { get; set; }
        public decimal DescuentoNotaCredito { get; set; }
        public decimal TotalNotaCredito { get; set; }
        public decimal CostoUnitarioNotaCredito { get; set; }
        public decimal CostoTotalNotaCredito { get; set; }
    }
}