namespace ApiAstilPos.Models
{
    public class ActividadesCiiu
    {
        public long IdActividadCiiu { get; set; }
        public string CodigoActividadCiiu { get; set; } = string.Empty;
        public string DescripcionActividadCiiu { get; set; } = string.Empty;
        public DateTime? FechaGrabacionActividadCiiu { get; set; }
    }
}
