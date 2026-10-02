using ApiAstilPos.Models;

public class Producto
{
        public long IdProducto { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }
        public string? ImagenProducto { get; set; }
        public string? CodigoBarras { get; set; }
        public long IdCategoria { get; set; }
        public long IdUnidadMedida { get; set; }
        public decimal? PrecioUnitario { get; set; }
        public decimal? StockActualProducto { get; set; }
        public decimal? CostoPromedioActualProducto { get; set; }
        public bool? ProductoActivo { get; set; }
        public decimal? PrecioPos { get; set; }

        public decimal? PorcentajeIva { get; set; }
        public decimal? PorcentajeImpoConsumo { get; set; }
        public decimal? PorcentajeReteIva { get; set; }
        public long IdTipoProducto { get; set; }
        public decimal? PorcentajeReteRenta { get; set; }
        public decimal? PorcentajeReteIca { get; set; }
        public decimal? PorcentajeMaxDescuento { get; set; }

        public DateTime? FechaGrabacionProducto { get; set; }
        public long? IdTerceroMandato { get; set; } = 0;
        public long? IdItemSector { get; set; } = 0;
        public bool? IndicadorMandato { get; set; } = false;
        public bool? ItemVenta { get; set; } = false;
        public bool? ItemCompra { get; set; } = false;
        public TributoProducto[] TributosProducto { get; set; }
        public PrecioProducto[] PreciosProducto { get; set; }
    
    
}