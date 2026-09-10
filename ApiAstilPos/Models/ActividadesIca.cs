namespace ApiAstilPos.Models
{
    public class ActividadesIca
    {
        public long IdActividadIca { get; set; }
        public string CodigoActividadIca { get; set; } = string.Empty;
        public string DescripcionActividadIca { get; set; } = string.Empty;
        public decimal? TarifaActividad { get; set; }
        public string? IdExterno { get; set; }
        public DateTime? FechaGrabacionActividadIca { get; set; }
    }
}
