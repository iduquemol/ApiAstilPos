namespace ApiAstilPos.Models
{
    public class FormaPago
    {
        public short IdFormaPago { get; set; }
        public string CodigoFormaPago { get; set; } = string.Empty;
        public string NombreFormaPago { get; set; } = string.Empty;
        public long? IdMedioPagoDefault { get; set; }
        public string? IdFormaPagoExterna { get; set; }
        public DateTime? FechaGrabacionFormaPago { get; set; }
    }
}
