using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiAstilPos.Models
{
    public class TipoProducto
    {
        public short IdTipoProducto { get; set; }
        public string? CodigoTipoProducto { get; set; }
        public string? NombreTipoProducto { get; set; }
        public bool ManejaInventario { get; set; }
    }
}
