#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class Vendedor
    {
        public long IdVendedor { get; set; }
        public string? CodigoVendedor { get; set; }
        public string? NombreVendedor { get; set; }
        public long? IdTerceroVendedor { get; set; }
        public DateTime? FechaGrabacionVendedor { get; set; }
    }
}