#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class DocumentoSoporte
    {
        public long IdDsa { get; set; }
        public long IdTipoDocumentoDsa { get; set; }
        public long NumeroDsa { get; set; }
        public string PrefijoDsa { get; set; } = string.Empty;
        public long IdTerceroDsa { get; set; }
        public DateTime FechaDsa { get; set; }
        public short? PlazoDiasDsa { get; set; }
        public DateTime? FechaVencimientoDsa { get; set; }
        public long? IdUsuario { get; set; }
        public long? IdFormaPagoDsa { get; set; }
        public string? OrdenReferenciaDsa { get; set; }
        public DateTime? FechaOrdenReferenciaDsa { get; set; }
        public string? ObservacionesDsa { get; set; }
        public long? TotalRegistrosDsa { get; set; }
        public decimal? CantidadProductosDsa { get; set; }
        public decimal? TotalPrecioDsa { get; set; }
        public decimal? TotalDescuentoDsa { get; set; }
        public decimal? TotalDsa { get; set; }
        public string? Cufe { get; set; }
        public string? FirmaDigital { get; set; }
        public DateTime? FechaHoraAutorizacion { get; set; }
        public long? IdResolucionDsa { get; set; }
        public long? IdResponseDianDsa { get; set; }
        public decimal? TotalBaseReteRentaDsa { get; set; }
        public decimal? TotalReteRentaDsa { get; set; }
        public decimal? TotalBaseReteIcaDsa { get; set; }
        public decimal? TotalReteIcaDsa { get; set; }
        public long? IdTipoOperacionDian { get; set; }
        public long? IdTipoDocumentoExterno { get; set; }
        public DateTime? FechaInicialServicio { get; set; }
        public DateTime? FechaFinalServicio { get; set; }

        public bool? ValidadoDian { get; set; }
        public DateTime? FechaGrabacionDsa { get; set; }

        public DocumentoSoporteTercero[]? TerceroDsa { get; set; }
        public DetalleDocumentoSoporte[]? DetalleDsa { get; set; }
    }
}