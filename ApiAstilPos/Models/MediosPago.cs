namespace ApiAstilPos.Models
{
    public class mediosPago
    {
        public short IdMedioPago { get; set; }
        public string CodigoMedioPago { get; set; }
        public string NombreMedioPago { get; set; }
        public string CodigoDianMedioPago { get; set; }
        public DateTime? FechaGrabacionMedioPago { get; set; }
        public string? IdMedioPagoExterno { get; set; }
    }
}