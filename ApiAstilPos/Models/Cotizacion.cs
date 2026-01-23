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
        public string IdTipoDocumento { get; set; }
        public string CodigoDocumento { get; set; }
        public string NombreDocumento { get; set; }
        public long NumeroCotizacion { get; set; }        
        public string PrefijoCotizacion { get; set; }
        public string FechaCotizacion { get; set; }        
        public long IdUsuario { get; set; }
        public long? TotalRegistros { get; set; }
        public decimal? CantidadProductos { get; set; }
        public decimal? TotalPrecio { get; set; }
        public decimal? TotalDescuento { get; set; }
        public decimal? TotalBaseIva { get; set; }
        public decimal? TotalIva { get; set; }
        public decimal? TotalCotizacion { get; set; }
        public VentaTercero TerceroCotizacion { get; set; }
        public CotizacionDetalle[] DetalleCotizacion { get; set; }        
    }
}
