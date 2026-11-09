using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiAstilPos.Models
{
    public class Cotizacion
    {
        public long IdCotizacion { get; set; }
        public long IdTipoDocumento { get; set; }
        public long? IdTipoDocumentoExterno { get; set; }
        public string CodigoDocumento { get; set; } = string.Empty;
        public string? NombreDocumento { get; set; }
        public long NumeroCotizacion { get; set; }
        public string? PrefijoCotizacion { get; set; }
        public long? IdTerceroCotizacion { get; set; }
        public DateTime FechaCotizacion { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public bool EsBorrador { get; set; }
        public long? IdUsuario { get; set; }
        public long? TotalRegistros { get; set; }
        public decimal? CantidadProductos { get; set; }
        public decimal? TotalPrecio { get; set; }
        public decimal? TotalDescuento { get; set; }
        public decimal? TotalBaseIva { get; set; }
        public decimal? TotalIva { get; set; }
        public decimal? TotalCotizacion { get; set; } 

        public decimal? TotalBaseReteIva { get; set; }
        public decimal? TotalReteIva { get; set; }
        public decimal? TotalBaseReteRenta { get; set; }
        public decimal? TotalReteRenta { get; set; }
        public decimal? TotalBaseReteIca { get; set; }
        public decimal? TotalReteIca { get; set; }

        public DateTime? FechaGrabacionCotizacion { get; set; }

        public string? Observaciones { get; set; }
        public string? OrdenReferencia { get; set; }
        public DateTime? FechaOrdenReferencia { get; set; }
        public CotizacionTercero TerceroCotizacion { get; set; }
        public CotizacionDetalle[] DetalleCotizacion { get; set; }        
    }
}
