using ApiAstilPos.Models;

public class Producto
{
    public long IdProducto { get; set; }
    public string CodigoProducto { get; set; }
    public string NombreProducto { get; set; }
    public string ImagenProducto { get; set; }
    public string CodigoBarras { get; set; }
    public long IdCategoria { get; set; }
    public long IdTipoProducto { get; set; }
    public long IdUnidadMedida { get; set; }

    // Precios y Costos
    public decimal PrecioUnitario { get; set; }
    public decimal PrecioPos { get; set; }
    public decimal? CostoPromedioActualProducto { get; set; }

    // Inventario
    public decimal? StockActualProducto { get; set; }

    // Descuentos y Porcentajes
    public decimal? PorcentajeMaxDescuento { get; set; }
    public decimal PorcentajeIva { get; set; }
    public decimal PorcentajeImpoConsumo { get; set; }
    public decimal PorcentajeReteIva { get; set; }
    public decimal PorcentajeReteRenta { get; set; }
    public decimal PorcentajeReteIca { get; set; }

    // Datos del Mandato y Sector
    public long? IdTerceroMandato { get; set; }
    public bool? IndicadorMandato { get; set; } = false;
    public long? IdItemSector { get; set; }

    // Estados
    public bool? ProductoActivo { get; set; } = false;

    // Listas / Relaciones de Detalles
    public TributoProducto[] TributosProducto { get; set; }
   
    //public PrecioProducto[] PreciosProducto { get; set; } 
}