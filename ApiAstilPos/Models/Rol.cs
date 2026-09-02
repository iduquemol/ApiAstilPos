#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class Rol
    {
        public long IdRol { get; set; }
        public string? CodigoRol { get; set; }
        public string? NombreRol { get; set; }
        public DateTime? FechaGrabacionRol { get; set; }
    }
}