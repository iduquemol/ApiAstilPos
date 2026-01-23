#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class TerceroVentaDefault
    {
        public long IdTercero { get; set; }
        public long IdTipoDocumentoId { get; set; }
        public string NombreTipoDocumentoId { get; set; }
        public string NumeroIdentificacion { get; set; }
        public string? PrimerNombre { get; set; }
        public string? PrimerApellido { get; set; }
        public string? RazonSocial { get; set; }
        public string? EmailTercero { get; set; }
    }
}