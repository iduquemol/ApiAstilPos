namespace ApiAstilPos.Models
{
    public class Consecutivo
    {
        public long IdConsecutivo { get; set; }
        public string NombreConsecutivo { get; set; } = string.Empty;
        public string? PrefijoConsecutivo { get; set; }
        public long NumeroInicial { get; set; }
        public long? NumeroFinal { get; set; }
        public long? NumeroActual { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public long? IdResolucion { get; set; }
        public long IdTipoDocumento { get; set; }
        public bool? ConsecutivoActivo { get; set; }
        public DateTime? FechaGrabacionConsecutivo { get; set; }
    }
}
