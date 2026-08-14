#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class Venta
    {
        public long IdVenta { get; set; }
        public long IdTipoDocumento { get; set; }
        public string? NombreDocumento { get; set; }
        public long IdFormaPago { get; set; }
        public long? IdMetodoDian { get; set; }
        public long NumeroVenta { get; set; }
        public string? PrefijoVenta { get; set; }
        public string FechaVenta { get; set; } = string.Empty;
        public bool EsBorrador { get; set; }
        public long IdPuntoVenta { get; set; }
        public long IdUsuario { get; set; }
        public long? TotalRegistros { get; set; }
        public decimal? CantidadProductos { get; set; }
        public decimal? TotalPrecio { get; set; }
        public decimal? TotalDescuento { get; set; }
        public decimal? TotalBaseIva { get; set; }
        public decimal? TotalIva { get; set; }
        public decimal? TotalVenta { get; set; }
        public VentaTercero TerceroVenta { get; set; }
        public VentaDetalle[] DetalleVenta { get; set; }
        public VentaMedioPago[] MediosPagoVenta { get; set; }
    }
}