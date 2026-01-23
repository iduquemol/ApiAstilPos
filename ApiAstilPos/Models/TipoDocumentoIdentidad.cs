#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class TipoDocumentoIdentidad
    {
        public short IdTipoDocumentoId { get; set; }
        public string? CodigoTipoDocumentoId { get; set; }
        public string? NombreTipoDocumentoId { get; set; }
        public string? ObservacionTipoDocumentoId { get; set; }
        public DateTime? FechaGrabacionTipoDocId { get; set; }
    }
}