#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class NotaCredito
    {
        public long IdNotaCredito { get; set; }
        public long IdTipoDocumento { get; set; }
        public long? NumeroNotaCredito { get; set; }
        public string? PrefijoNotaCredito { get; set; }
        public long IdTerceroNotaCredito { get; set; }
        public DateTime? FechaNotaCredito { get; set; }
        public long? IdPuntoVenta { get; set; }
        public long? IdUsuario { get; set; }
        public long? IdVenta { get; set; }
        public long? IdConceptoCorreccionNota { get; set; }
        public string? Observaciones { get; set; }
        public long? TotalRegistros { get; set; }
        public decimal? CantidadProductos { get; set; }
        public decimal? TotalPrecio { get; set; }
        public decimal? TotalDescuento { get; set; }
        public decimal? TotalBaseIva { get; set; }
        public decimal? TotalIva { get; set; }
        public decimal? TotalNotaCredito { get; set; }
        public long? IdConceptoCorreccionDian { get; set; }
        public string? Cufe { get; set; }
        public string? FirmaDigital { get; set; }
        public DateTime? FechaHoraAutorizacion { get; set; }
        public long? IdResponseDian { get; set; }
        public DateTime? FechaGrabacionNotaCredito { get; set; }
        public decimal? TotalBaseRetelva { get; set; }
        public decimal? TotalRetelva { get; set; }
        public decimal? TotalBaseReteRenta { get; set; }
        public decimal? TotalReteRenta { get; set; }
        public decimal? TotalBaseRetelca { get; set; }
        public decimal? TotalRetelca { get; set; }
        public long? IdTipoOperacionDian { get; set; }
        public long? IdResolucion { get; set; }
        public short? IdFormaPago { get; set; }
        public short? PlazoDias { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public long? IdTipoDocumentoExterno { get; set; }
        public NotaCreditoDetalle[] DetalleNotaCredito { get; set; }

        public string? CodigoDocumento { get; set; }
        public string? NombreDocumento { get; set; }
        public string? NumeroIdentificacionTerceroNotaCredito { get; set; }
        public string? NombreTerceroNotaCredito { get; set; }
    }
}