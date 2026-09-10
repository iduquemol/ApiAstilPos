#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class TipoDocumentoIdentidad
    {
        public short IdTipoDocumentoId { get; set; }
        public string CodigoTipoDocumentoId { get; set; } = string.Empty;
        public string? NombreTipoDocumentoId { get; set; }
        public string? ObservacionTipoDocumentoId { get; set; }
        public long? IdTipoDocumentoFE { get; set; }
        public short? IdTipoPersona { get; set; }
        public string? NombreTipoPersona { get; set; } // Incluido por el LEFT JOIN del SP
        public DateTime? FechaGrabacionTipoDocId { get; set; }
        public string? IdExterno { get; set; }
    }
}