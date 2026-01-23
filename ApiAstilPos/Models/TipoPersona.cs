#nullable enable
using System;

namespace azureFunctionPos.Models
{
    public class TipoPersona
    {
        public short IdTipoPersona { get; set; }
        public string? CodigoTipoPersona { get; set; }
        public string? NombreTipoPersona { get; set; }
    }
}