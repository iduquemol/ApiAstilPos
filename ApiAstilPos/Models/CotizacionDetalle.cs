using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiAstilPos.Models
{
    public class CotizacionDetalle
    {
        public long IdDetalleCotizacion { get; set; }
        public long RegistroCotizacion { get; set; }
        public long IdProducto { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }
        public decimal CantidadCotizacion { get; set; }
        public decimal PrecioUnitarioCotizacion { get; set; }
        public decimal PorcentajeIvaCotizacion { get; set; }
        public decimal IvaCotizacion { get; set; }
        public decimal PorcentajeDescuentoCotizacion { get; set; }
        public decimal DescuentoCotizacion { get; set; }
        public decimal TotalCotizacion { get; set; }
        public decimal CostoUnitarioCotizacion { get; set; }
        public decimal CostoTotalCotizacion { get; set; }
    }
}
