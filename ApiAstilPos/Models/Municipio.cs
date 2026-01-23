#nullable enable
using System;

namespace azureFunctionPos.Models
{
    public class DepartamentoMunicipio
    {
        public long IdDepartamento { get; set; }
        public string CodigoDepartamento { get; set; }
        public string NombreDepartamento { get; set; }
        public Municipio[] Municipios { get; set; }
    }

    public class Municipio
    {
        public long IdMunicipio { get; set; }
        public string? CodigoMunicipio { get; set; }
        public string? NombreMunicipio { get; set; }
        public long IdMunicipioFe { get; set; }
        
    }
}