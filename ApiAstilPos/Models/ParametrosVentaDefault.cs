using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiAstilPos.Models
{
    public class ParametrosVentaDefault
    {
        public TerceroVentaDefault[] TerceroVenta { get; set; }
        public TipoDocumentoDefault[] DocumentoVenta { get; set; }
        public TipoDocumentoDefault[] DocumentoNotaCredito { get; set; }
        public TipoDocumentoDefault[] DocumentoCotizacion { get; set; }
    }
}
