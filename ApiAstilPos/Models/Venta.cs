#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class Venta
    {
        #nullable enable
        public long IdVenta { get; set; }
        public long IdTipoDocumento { get; set; }
        public long? IdTipoDocumentoExterno { get; set; }
        public string CodigoDocumento { get; set; } = string.Empty;
        public string? NombreDocumento { get; set; }
        public long? IdMetodoDian { get; set; }
        public short? IdFormaPago { get; set; } 
        public long NumeroVenta { get; set; }
        public string? PrefijoVenta { get; set; }
        public long? IdTerceroVenta { get; set; }
        public DateTime? FechaVenta { get; set; } 
        public short? PlazoDias { get; set; } 
        public DateTime? FechaVencimiento { get; set; }
        public bool EsBorrador { get; set; }
        public long? IdPuntoVenta { get; set; }
        public long? IdUsuario { get; set; }
        public long? TotalRegistros { get; set; }
        public decimal? CantidadProductos { get; set; }
        public decimal? TotalPrecio { get; set; }
        public decimal? TotalDescuento { get; set; }
        public decimal? TotalBaseIva { get; set; }
        public decimal? TotalIva { get; set; }
        public decimal? TotalVenta { get; set; }

        public decimal? TotalBaseReteIva { get; set; }
        public decimal? TotalReteIva { get; set; }
        public decimal? TotalBaseReteRenta { get; set; }
        public decimal? TotalReteRenta { get; set; }
        public decimal? TotalBaseReteIca { get; set; }
        public decimal? TotalReteIca { get; set; }

        public string? EstadoDian { get; set; }
        public string? Cufe { get; set; }
        public string? FirmaDigital { get; set; }
        public DateTime? FechaHoraAutorizacion { get; set; }
        public long? IdResolucion { get; set; }
        public long? IdResponseDian { get; set; }
        public long? IdTipoOperacionDian { get; set; }
        public DateTime? FechaGrabacionVenta { get; set; }

        public string? Observaciones { get; set; }
        public string? OrdenReferencia { get; set; }
        public DateTime? FechaOrdenReferencia { get; set; }

            
        public VentaTercero TerceroVenta { get; set; }
        public VentaDetalle[] DetalleVenta { get; set; }
        public VentaMedioPago[] MediosPagoVenta { get; set; }
    }
}