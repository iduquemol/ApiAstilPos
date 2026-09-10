using System;

namespace ApiAstilPos.Models
{
    public class TipoDocumento
    {
        public long IdTipoDocumento { get; set; }
        public string? CodigoDocumento { get; set; }
        public string? NombreDocumento { get; set; }
        public long? IdTipoDocumentoE { get; set; }
        public string? NombreDocumentoE { get; set; }
        public long? IdFormaPago { get; set; }
        public string? NombreFormaPago { get; set; }
        public long? IdMetodoDian { get; set; }
        public string? NombreMetodo { get; set; }
        public bool? DocumentoVenta { get; set; }
        public bool? DocumentoNotaCredito { get; set; }
        public long? OrdenTipoDocumento { get; set; }
        public bool? TipoDocumentoActivo { get; set; }
        public bool? DocumentoRemision { get; set; }
        public bool? DocumentoCotizacion { get; set; }
        public long? IdTipoDocumentoNC { get; set; }
        public long? IdTipoDocumentoCotiza { get; set; }
        public long? IdTipoDocumentoND { get; set; }
        public long? IdConsecutivoHabilitacion { get; set; }
   
        public long IdTipoDocumentoExterno { get; set; }
        public string? NombreTipoDocumentoExterno { get; set; }
        public string? CodigoTipoDocumentoExterno { get; set; }
    }
}