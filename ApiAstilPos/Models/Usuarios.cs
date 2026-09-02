#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class Usuario
    {
        public long IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public long IdRolUsuario { get; set; }
        public string PassUsuario { get; set; } = string.Empty;
        public string EmailUsuario { get; set; } = string.Empty;
        public long? IdVendedor { get; set; }
        public long? IdSucursalUsuario { get; set; }
        public DateTime? FechaGrabacionUsuario { get; set; }
    }
}