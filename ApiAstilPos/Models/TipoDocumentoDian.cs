#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class TipoDocumentoDian
    {
        public long IdTipoDocumentoE { get; set; }
        public string CodigoDocumentoE { get; set; } = string.Empty;
        public string NombreDocumentoE { get; set; } = string.Empty;
        public string? ObservacionDocumentoE { get; set; }
        public bool? AdmiteEventosDian { get; set; }
        public long? IdTipoDocumentoFe { get; set; }
        public long? IdMetodoDian { get; set; }
        public DateTime? FechaGrabacionTipoDocumentoE { get; set; }
    }
}