#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class RolOpcionServicio
    {
        public long IdRolOpcion { get; set; }
        public long IdRol { get; set; }
        public string? NombreRol { get; set; }
        public long IdOpcion { get; set; }
        public string? NombreOpcion { get; set; }
        public bool PuedeCrear { get; set; }
        public bool PuedeLeer { get; set; }
        public bool PuedeActualizar { get; set; }
        public bool PuedeEliminar { get; set; }
        public DateTime? FechaGrabacionRolOpcion { get; set; }
    }
}