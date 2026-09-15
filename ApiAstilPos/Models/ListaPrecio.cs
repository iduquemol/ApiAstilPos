using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azureFunctionPos.Models
{
    public class ListaPrecio
    {
        public long IdListaPrecio { get; set; }
        public string CodigoListaPrecio { get; set; }
        public string NombreListaPrecio { get; set; }
        public DateTime? FechaIniciaVigencia { get; set; }
        public DateTime? FechaFinalVigencia { get; set; }
        public bool? ListaPreciosActiva { get; set; }
        public DateTime? FechaGrabacionListaPrecio { get; set; }
    }
}
