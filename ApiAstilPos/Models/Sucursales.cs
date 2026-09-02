#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class Sucursal
    {
        public long IdSucursal { get; set; }
        public string? CodigoSucursal { get; set; }
        public string? NombreSucursal { get; set; }

        public long? IdDepartamentoSucursal { get; set; }

        public long? IdMunicipioSucursal { get; set; }

        public DateTime? FechaGrabacionSucursal { get; set; }
    }
}