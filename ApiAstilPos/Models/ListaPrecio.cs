using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace azureFunctionPos.Models
{
    public class ListaPrecio
    {
        public short IdListaPrecio { get; set; }
        public string? CodigoListaPrecio { get; set; }
        public string? NombreListaPrecio { get; set; }        
    }
}
