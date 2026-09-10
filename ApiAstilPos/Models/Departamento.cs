#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class Departamento
    {
        public long IdDepartamento { get; set; }
        public string? CodigoDepartamento { get; set; }
        public string? NombreDepartamento { get; set; }
        public long? IdDepartamentoFe { get; set; }
        public DateTime? FechaGrabacionDepartamento { get; set; }
    }
}