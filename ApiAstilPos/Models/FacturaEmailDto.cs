namespace ApiAstilPos.Models
{
    public class FacturaEmailDto
    {
        public string Email { get; set; }
        public string NombreCliente { get; set; }
        public string NumeroDocumento { get; set; }
        public string FacturadorNombre { get; set; }
        public string SubjectEmail { get; set; } = string.Empty;
        public decimal? Total { get; set; }
        public byte[] PdfAttachment { get; set; }
        public string PdfFileName { get; set; }
        public byte[] XmlAttachment { get; set; } 
        public string XmlFileName { get; set; }
    }
}