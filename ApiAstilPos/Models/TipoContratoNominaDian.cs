#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class TipoContratoNominaDian
    {
        public long IdTipoContratoDian { get; set; }
        public string? CodigoTipoContrato { get; set; } = string.Empty;
        public string? NombreTipoContratoDian { get; set; } = string.Empty;
        public string? DescripcionTipoContrato { get; set; } = string.Empty;
    }
}