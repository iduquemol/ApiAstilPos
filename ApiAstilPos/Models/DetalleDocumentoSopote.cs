#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class DetalleDocumentoSoporte
    {
        public long IdDetalleDsa { get; set; }
        public long IdDsa { get; set; }
        public long RegistroDsa { get; set; }
        public long IdProducto { get; set; }

        public string? CodigoProducto { get; set; }

        public decimal? CantidadDsa { get; set; }
        public decimal? PrecioUnitarioDsa { get; set; }
        public decimal? PrecioTotalDsa { get; set; }
        public decimal? PorcentajeDescuentoDsa { get; set; }
        public decimal? DescuentoDsa { get; set; }
        public decimal? TotalDsa { get; set; }
        public decimal? CostoUnitarioDsa { get; set; }
        public decimal? CostoTotalDsa { get; set; }
        public decimal? PorcentajeReteRenta { get; set; }
        public decimal? BaseReteRenta { get; set; }
        public decimal? ReteRentaDsa { get; set; }
        public decimal? PorcentajeReteIca { get; set; }
        public decimal? BaseReteIca { get; set; }
        public decimal? ReteIcaDsa { get; set; }
        public string? NombreProducto { get; set; }
        public DateTime? FechaGrabacionDetalleDsa { get; set; }
    }
}