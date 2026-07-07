namespace ApiAstilPos.Models
{
    public class RespuestaConsultaFactura
    {
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public string Numeracion { get; set; }
        public string NumeroIdentificacion { get; set; }
        public string RazonSocial { get; set; }
        public decimal Monto { get; set; }
        public long IdVenta { get; set; }
        public long IdMetodoDian { get; set; }
        public long IdTipoDocumento { get; set; }
        public string Num_doc { get; set; }

    }
}
