namespace ApiAstilPos.Models
{
    public class Categorias
    {
        public long? idCategoria { get; set; }
        public string? codigoCategoria { get; set; }
        public string? nombreCategoria { get; set; }
        public string? iconoCategoria { get; set; }
        public long? idTarifaTributo { get; set; }
        public string? nombreTarifa { get; set; }
        public decimal? tarifa { get; set; }
        public DateTime? fechaGrabacionCategoria { get; set; }
    }
}