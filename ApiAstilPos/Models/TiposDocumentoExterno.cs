#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class TipoDocumentoExterno
    {
        public long IdTipoDocumentoExterno { get; set; }
        public string CodigoTipoDocumentoExterno { get; set; } = string.Empty;
        public string NombreTipoDocumentoExterno { get; set; } = string.Empty;
        public long IdTipoDocumento { get; set; }

        // Campos opcionales (Nullables)
        public string? NotaFe1Externo { get; set; }
        public string? NotaFe2Externo { get; set; }
        public string? NotaFe3Externo { get; set; }
        public string? NotaFe4Externo { get; set; }
        public string? NotaFe5Externo { get; set; }

        public long? IdConsecutivo { get; set; }
        public long? IdFormaPago { get; set; }
        public bool? TipoDocumentoActivo { get; set; }

        public DateTime? FechaGrabacionDocumentoExterno { get; set; }
    }
}