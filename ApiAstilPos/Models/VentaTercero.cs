#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class VentaTercero
    {
        public long IdTercero { get; set; }
        public long IdTipoDocumentoId { get; set; }
        public string? DigitoVerificacion { get; set; }
        public string NumeroIdentificacion { get; set; }
        public string PrimerNombre { get; set; }
        public string PrimerApellido { get; set; }
        public string RazonSocial { get; set; }
        public string? TelefonoTercero { get; set; }
        public long? IdMunicipio { get; set; }
        public string? EmailTercero { get; set; }
        public long? IdTipoPersona { get; set; }
        public bool? TerceroGeneral { get; set; }
    }
}