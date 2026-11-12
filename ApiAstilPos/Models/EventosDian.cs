using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiAstilPos.Models
{
    public class EventosDian
    {
        public long IdEventoDian { get; set; }
        public string? NombreEventoDian { get; set; }
        public string? CodigoEventoDian { get; set; }
        public string? DetalleEventoDian { get; set; }
        public long? IdActorEvento { get; set; }
        public DateTime? FechaGrabacionEventoDian { get; set; }
    }
}
