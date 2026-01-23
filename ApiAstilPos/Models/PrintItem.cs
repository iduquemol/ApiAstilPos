using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApiAstilPos.Models
{
    public class PrintItem
    {
        public int RegistroVenta { get; set; }

        public string CodigoProducto { get; set; }

        public decimal CantidadVenta { get; set; }

        public string NombreProducto { get; set; }

        public string CodigoUnidadMedida { get; set; }

        public string NombreUnidadMedida { get; set; }

        public decimal ValorReferenciaUnidad { get; set; }

        public decimal PorcentajeDescuentoVenta { get; set; }

        public decimal PorcentajeIvaVenta { get; set; }

        public decimal PrecioUnitarioVenta { get; set; }

        public decimal PrecioTotalVenta { get; set; }
    }
}
