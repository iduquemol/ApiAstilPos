#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class Vendedor
    {
        public long IdVendedor { get; set; }
        public string CodigoVendedor { get; set; }
        public string NombreVendedor { get; set; }
        public long? idTerceroVendedor { get; set; }
        public string NombreTerceroVendedor { get; set; }
    }
}