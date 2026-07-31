#nullable enable
using System;
using ApiAstilPos.Models;

namespace ApiAstilPos.Models
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

        public CodigoPostal[]? CodigosPostales { get; set; }
    }
}