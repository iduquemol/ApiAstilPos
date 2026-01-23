#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class NotaCredito
    {
        public long? IdNotaCredito { get; set; }
        public string IdTipoDocumento { get; set; }
        public string CodigoDocumento { get; set; }
        public string NombreDocumento { get; set; }
        public long? NumeroNotaCredito { get; set; }        
        public string PrefijoNotaCredito { get; set; }
        public long? ConceptoNotaCredito { get; set; }
        public string FechaNotaCredito { get; set; }        
        public long IdUsuario { get; set; }
        public long? TotalRegistros { get; set; }
        public decimal? CantidadProductos { get; set; }
        public decimal? TotalPrecio { get; set; }
        public decimal? TotalDescuento { get; set; }
        public decimal? TotalBaseIva { get; set; }
        public decimal? TotalIva { get; set; }
        public decimal? TotalVenta { get; set; }
        public long? IdTerceroNotaCredito { get; set; }
        public string NumeroIdentificacionTerceroNotaCredito { get; set; }
        public string NombreTerceroNotaCredito { get; set; }
        public long? IdVenta { get; set; }
        public long? IdConceptoCorreccionNota { get; set; }
        public NotaCreditoDetalle[] DetalleNotaCredito { get; set; }
        
    }
}