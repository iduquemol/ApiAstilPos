using System;

namespace azureFunctionPos.Models
{
    public class Categoria
    {
        public long IdCategoria { get; set; }
        public string CodigoCategoria { get; set; }
        public string NombreCategoria { get; set; }
        public string IconoCategoria { get; set; }        
    }
}