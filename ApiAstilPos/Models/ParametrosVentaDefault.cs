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
        public ListaPrecioDto[] ListaPrecios { get; set; }
        public TipoRegimenDto[] TipoRegimen { get; set; }
    }

    public class ListaPrecioDto
    {
        public long IdListaPrecio { get; set; }
        public string NombreListaPrecio { get; set; }
    }

    public class TipoRegimenDto
    {
        public long IdTipoRegimen { get; set; }
        public string NombreRegimen { get; set; }
    }
}
