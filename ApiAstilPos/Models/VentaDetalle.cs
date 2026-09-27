using System;

namespace ApiAstilPos.Models
{
    public class VentaDetalle
    {
        public long IdDetalleVenta { get; set; }
        public long IdVenta { get; set; }
        public long RegistroVenta { get; set; }
        public long? IdProducto { get; set; }
        public decimal? CantidadVenta { get; set; }
        public decimal? PrecioUnitarioVenta { get; set; }
        public decimal? PrecioTotalVenta { get; set; }
        public decimal? PorcentajeDescuentoVenta { get; set; }
        public decimal? DescuentoVenta { get; set; }
        public long? PorcentajeImpoConsumo { get; set; }
        public long? ImpoConsumoVenta { get; set; }
        public decimal? BaseIvaVenta { get; set; }
        public decimal? PorcentajeIvaVenta { get; set; }
        public decimal? IvaVenta { get; set; }
        public decimal? TotalVenta { get; set; }
        public decimal? CostoUnitarioVenta { get; set; }
        public decimal? CostoTotalVenta { get; set; }
        public DateTime? FechaGrabacionDetalleVenta { get; set; }
        public decimal? PorcentajeReteIva { get; set; }
        public decimal? BaseReteIvaVenta { get; set; }
        public decimal? ReteIvaVenta { get; set; }
        public decimal? PorcentajeReteRenta { get; set; }
        public decimal? BaseReteRenta { get; set; }
        public decimal? ReteRentaVenta { get; set; }

        public decimal? PorcentajeReteIca { get; set; }
        public decimal? BaseReteIca { get; set; }
        public decimal? ReteIcaVenta { get; set; }
        public string? NombreProducto { get; set; }
        public long? IdTerceroMandato { get; set; }
        public long? IdItemSector { get; set; }
        public bool? IndicadorMuestra { get; set; }
        public decimal? ValorReferenciaUnidad { get; set; }
        public decimal? ValorReferenciaTotal { get; set; }

        public string? CodigoProducto { get; set; }
        public long? IdTipoProducto { get; set; }
        public long? CantidadNotaCredito { get; set; }
        public bool? IntNotaCredito { get; set; }
    }
}