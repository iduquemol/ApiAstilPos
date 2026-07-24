namespace ApiAstilPos.Models
{
    public class PrecioProducto
    {
        public long? IdPrecioProducto { get; set; }
        public long? IdProducto { get; set; } 
        public long? IdListaPrecio { get; set; }
        public decimal? Precio { get; set; }         
    }
}
