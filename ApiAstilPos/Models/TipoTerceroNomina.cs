#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class TipoTerceroNomina
    {
        public long IdTipoTercero { get; set; }
        public string? NombreTipoTercero { get; set; } = string.Empty;
        public DateTime? FechaGrabacionTipoTercero { get; set; }
    }
}