#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class UnidadDeMedida
    {
        public long IdUnidadMedida { get; set; }
        public string? CodigoUnidadMedida { get; set; }
        public string? NombreUnidadMedida { get; set; }
        public DateTime? FechaGrabacionUnidadMedida { get; set; }
    }
}