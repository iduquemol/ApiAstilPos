#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class GrupoConceptoNominaDian
    {
        public long IdGrupoConcepto { get; set; }
        public string? NombreGrupoConcepto { get; set; } = string.Empty;
        public long IdNaturaleza { get; set; }
        public bool? ReportarDian { get; set; }
        public DateTime? FechaGrabacionGrupoConcepto { get; set; }
    }
}