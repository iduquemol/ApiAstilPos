#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class NaturalezaConceptoNomina
    {
        public long IdNaturalezaConcepto { get; set; }
        public string? NombreNaturalezaConcepto { get; set; } = string.Empty;
        public int? NaturalezaConcepto { get; set; }
        public DateTime? FechaGrabacionNaturaleza { get; set; }
    }
}