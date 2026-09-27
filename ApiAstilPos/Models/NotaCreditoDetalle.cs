#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class NotaCreditoDetalle
    {
        public long IdDetalleNotaCredito { get; set; }
        public long IdNotaCredito { get; set; }
        public long RegistroNotaCredito { get; set; }
        public long? IdDetalleVenta { get; set; }
        public long? IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public decimal? CantidadNotaCredito { get; set; }
        public decimal? PrecioUnitarioNotaCredito { get; set; }
        public decimal? PrecioTotalNotaCredito { get; set; }
        public decimal? PorcentajeDescuentoNotaCredito { get; set; }
        public decimal? DescuentoNotaCredito { get; set; }
        public decimal? BaseIvaNotaCredito { get; set; }
        public decimal? PorcentajeIvaNotaCredito { get; set; }
        public decimal? IvaNotaCredito { get; set; }
        public decimal? TotalNotaCredito { get; set; }
        public decimal? CostoUnitarioNotaCredito { get; set; }
        public decimal? CostoTotalNotaCredito { get; set; }
        public DateTime? FechaGrabacionDetalleNotaCredito { get; set; }
        public decimal? PorcentajeRetelva { get; set; }
        public decimal? BaseRetelvaNotaCredito { get; set; }
        public decimal? RetelvaNotaCredito { get; set; }
        public decimal? PorcentajeReteRenta { get; set; }
        public decimal? BaseReteRenta { get; set; }
        public decimal? ReteRentaNotaCredito { get; set; }
        public decimal? PorcentajeRetelca { get; set; }
        public decimal? BaseRetelca { get; set; }
        public decimal? RetelcaNotaCredito { get; set; }
        public long? IdTerceroMandato { get; set; }
        public long? IdItemSector { get; set; }
        public bool? IndicadorMuestra { get; set; }
        public decimal? ValorReferenciaUnidad { get; set; }
        public decimal? ValorReferenciaTotal { get; set; }

        public string? CodigoProducto { get; set; }
    }
}