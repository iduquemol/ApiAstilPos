namespace ApiAstilPos.Models
{
    public class Almacenes
    {
        public long IdAlmacen { get; set; }
        public string CodigoAlmacen { get; set; } = string.Empty;
        public string NombreAlmacen { get; set; } = string.Empty;
        public long IdSucursal { get; set; }
        public string? DireccionAlmacen { get; set; }
        public long? IdTerceroResponsable { get; set; }
        public DateTime? FechaGrabacionAlmacen { get; set; }
        public string? NombreSucursal { get; set; }
        public string? RazonSocial { get; set; }

    }
}
