namespace ApiAstilPos.Models
{
    public class TributosCategoria
    {
        public long? idTributoCategoria { get; set; }
        public long? idCategoria { get; set; }
        public long idTributo { get; set; }
        public long idTarifaTributo { get; set; }
    }
}