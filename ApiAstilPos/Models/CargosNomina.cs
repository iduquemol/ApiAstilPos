using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiAstilPos.Models
{
    public class CargosNomina
    {
        public long IdCargoEmpleado { get; set; }
        public string? CodigoCargoEmpleado { get; set; }
        public string? NombreCargoEmpleado { get; set; }
        public DateTime? FechaGrabacionCargoEmpleado { get; set; }
    }
}
