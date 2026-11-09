#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class NotaDebitoDetalle
    {
        public long IdDetalleNotaDebito { get; set; }
        public long IdNotaDebito { get; set; }
        public long RegistroNotaDebito { get; set; }
        public long? IdDetalleVenta { get; set; }
        public long? IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public decimal? CantidadNotaDebito { get; set; }
        public decimal? PrecioUnitarioNotaDebito { get; set; }
        public decimal? PrecioTotalNotaDebito { get; set; }
        public decimal? PorcentajeDescuentoNotaDebito { get; set; }
        public decimal? DescuentoNotaDebito { get; set; }
        public decimal? BaseIvaNotaDebito { get; set; }
        public decimal? PorcentajeIvaNotaDebito { get; set; }
        public decimal? IvaNotaDebito { get; set; }
        public decimal? TotalNotaDebito { get; set; }
        public decimal? CostoUnitarioNotaDebito { get; set; }
        public decimal? CostoTotalNotaDebito { get; set; }
        public DateTime? FechaGrabacionDetalleNotaDebito { get; set; }
        public decimal? PorcentajeRetelva { get; set; }
        public decimal? BaseRetelvaNotaDebito { get; set; }
        public decimal? RetelvaNotaDebito { get; set; }
        public decimal? PorcentajeReteRenta { get; set; }
        public decimal? BaseReteRenta { get; set; }
        public decimal? ReteRentaNotaDebito { get; set; }
        public decimal? PorcentajeRetelca { get; set; }
        public decimal? BaseRetelca { get; set; }
        public decimal? RetelcaNotaDebito { get; set; }
        public long? IdTerceroMandato { get; set; }
        public long? IdItemSector { get; set; }
        public bool? IndicadorMuestra { get; set; }
        public decimal? ValorReferenciaUnidad { get; set; }
        public decimal? ValorReferenciaTotal { get; set; }

        public string? CodigoProducto { get; set; }
    }
}