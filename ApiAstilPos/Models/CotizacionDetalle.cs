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
        public long IdCotizacion { get; set; }
        public long RegistroCotizacion { get; set; }
        public long? IdProducto { get; set; }
        public decimal? CantidadCotizacion { get; set; }
        public decimal? PrecioUnitarioCotizacion { get; set; }
        public decimal? PrecioTotalCotizacion { get; set; }
        public decimal? PorcentajeDescuentoCotizacion { get; set; }
        public decimal? DescuentoCotizacion { get; set; }
        public long? PorcentajeImpoConsumo { get; set; }
        public long? ImpoConsumoCotizacion { get; set; }
        public decimal? BaseIvaCotizacion { get; set; }
        public decimal? PorcentajeIvaCotizacion { get; set; }
        public decimal? IvaCotizacion { get; set; }
        public decimal? TotalCotizacion { get; set; }
        public decimal? CostoUnitarioCotizacion { get; set; }
        public decimal? CostoTotalCotizacion { get; set; }
        public DateTime? FechaGrabacionDetalleCotizacion { get; set; }
        public decimal? PorcentajeReteIva { get; set; }
        public decimal? BaseReteIvaCotizacion { get; set; }
        public decimal? ReteIvaCotizacion { get; set; }
        public decimal? PorcentajeReteRenta { get; set; }
        public decimal? BaseReteRenta { get; set; }
        public decimal? ReteRentaCotizacion { get; set; }

        public decimal? PorcentajeReteIca { get; set; }
        public decimal? BaseReteIca { get; set; }
        public decimal? ReteIcaCotizacion { get; set; }
        public string? NombreProducto { get; set; }
        public long? IdTerceroMandato { get; set; }
        public long? IdItemSector { get; set; }
        public bool? IndicadorMuestra { get; set; }
        public decimal? ValorReferenciaUnidad { get; set; }
        public decimal? ValorReferenciaTotal { get; set; }

        public string? CodigoProducto { get; set; }
        public long? IdTipoProducto { get; set; }
    }
}
