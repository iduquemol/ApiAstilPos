namespace ApiAstilPos.Models
{
    public class EnviarEvento
    {
        public long IdTipoDocumentoExterno { get; set; } = 7;
        public long IdVenta { get; set; }
        public long IdUsuario { get; set; } = 1;
        public long IdEventoDian { get; set; }
        public long? IdConceptoRechazo { get; set; }
    }
}