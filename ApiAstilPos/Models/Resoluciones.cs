using System.ComponentModel.DataAnnotations;

namespace ApiAstilPos.Models
{
    public class resoluciones
    {
        public long idResolucion { get; set; }

        public string numeroResolucion { get; set; }

        public string nombreResolucion { get; set; }

        public string claveTecnica { get; set; }

        public DateTime fechaAutorizacion { get; set; }

        public short vigenciaMeses { get; set; }

        public DateTime fechaInicial { get; set; }

        public DateTime fechaVencimiento { get; set; }

        public string prefijoResolucion { get; set; }

        public long numeroInicialResolucion { get; set; }

        public long numeroFinalResolucion { get; set; }

        public long numeroActual { get; set; }

        public bool resolucionActiva { get; set; }

        public long idTipoDocumentoDian { get; set; }
    }
}