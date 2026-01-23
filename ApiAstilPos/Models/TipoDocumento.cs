#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class TipoDocumento
    {
        public long IdTipoDocumento { get; set; }
        public string CodigoDocumento { get; set; }
        public string NombreDocumento { get; set; }
        public long? IdTipoDocumentoE { get; set; }   
        public long? IdFormaPago { get; set; }
        public string NombreFormaPago { get; set; }
        public long? IdMetodoDian { get; set; }
        public string NombreMetodo { get; set; }
    }
}