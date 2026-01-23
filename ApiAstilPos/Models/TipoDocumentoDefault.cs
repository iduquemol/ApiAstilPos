using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiAstilPos.Models
{
    public class TipoDocumentoDefault
    {
        public long IdTipoDocumento { get; set; }
        public string CodigoDocumento { get; set; }
        public string NombreDocumento { get; set; }
        public long? IdTipoDocumentoE { get; set; }
        public long? IdFormaPago { get; set; }
        public string NombreFormaPago { get; set; }
        public long? IdMetodoDian { get; set; }
        public string NombreMetodo { get; set; }
        public long OrdenTipoDocumento { get; set; }
        public bool TipoDocumentoActivo { get; set; }
    }
}
