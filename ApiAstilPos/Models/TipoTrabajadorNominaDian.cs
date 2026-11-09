#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class TipoTrabajadorNominaDian
    {
        public long IdTipoTrabajadorDian { get; set; }
        public string? CodigoTipoTrabajadorDian { get; set; } = string.Empty;
        public string? NombreTipoTrabajadorDian { get; set; } = string.Empty;
        public DateTime? FechaGrabacionTipoTrabajadorDian { get; set; }
    }
}