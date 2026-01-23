#nullable enable
using System;

namespace azureFunctionPos.Models
{
    public class DocumentoLista
    {
        public long IdVenta { get; set; }
        public string Documento { get; set; }
        public string NombreFormaPago { get; set; }
        public string NumeroDocumento { get; set; }
        public string NumeroIdentificacion { get; set; }
        public string NombreCliente { get; set; }
        public DateTime FechaVenta { get; set; }
        public long? TotalVenta { get; set; }             
    }
}