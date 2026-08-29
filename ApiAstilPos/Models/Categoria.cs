namespace ApiAstilPos.Models
{
    public class Categorias
    {
        public long? idCategoria { get; set; }
        public string? codigoCategoria { get; set; }
        public string? nombreCategoria { get; set; }
        public string? iconoCategoria { get; set; }
        public bool? categoriaActiva { get; set; } = false;
        public DateTime? fechaGrabacionCategoria { get; set; }
        public List<TributosCategoria>? tributosCategoria { get; set; } = new List<TributosCategoria>();
    }
}