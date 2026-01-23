using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApiAstilPos.Models
{
    public class PrintVenta
    {
        public string FacturadorNombre { get; set; }

        public string? FacturadorNombreComercial { get; set; }

        public string FacturadorTipoId { get; set; }

        public string FacturadorNumeroIdentificacion { get; set; }

        public string FacturadorTipoContribuyente { get; set; }

        public string FacturadorResponsabilidadFiscal { get; set; }

        public string FacturadorTipoRegimen { get; set; }

        public string FacturadorMunicipio { get; set; }

        public string FacturadorDireccion { get; set; }

        public string FacturadorEmail { get; set; }

        public long FacturadorTelefono { get; set; }

        public string ClienteRazonSocial { get; set; }

        public string ClienteTipoId { get; set; }

        public string ClienteNumeroIdentificacion { get; set; }

        public string? ClienteTipoContribuyente { get; set; }

        public string ClienteResponsabilidadFiscal { get; set; }

        public string? ClienteTipoRegimen { get; set; }

        public string ClienteMunicipio { get; set; }

        public string? ClienteDireccion { get; set; }

        public string ClienteEmail { get; set; }

        public long? ClienteTelefono { get; set; }

        public string FechaVenta { get; set; }
        public string FechaEntrega { get; set; }

        public string? FechaHoraAutorizacion { get; set; }

        public string PrefijoVenta { get; set; }

        public long NumeroVenta { get; set; }

        public string? Cufe { get; set; }

        public string? FirmaDigital { get; set; }

        public string NumeroResolucion { get; set; }

        public string FechaAutorizacionResolucion { get; set; }

        public string FechaInicialResolucion { get; set; }

        public string FechaFinalResolucion { get; set; }

        public long NumeroInicialResolucion { get; set; }

        public long NumeroFinalResolucion { get; set; }

        public string NombreFormaPago { get; set; }
        public string TipoOperacion { get; set; }
        public string NombreMedioPago { get; set; }

        public string NombreUsuario { get; set; }

        public int PlazoDias { get; set; }

        public string? FechaVencimiento { get; set; }

        public string? OrdenReferencia { get; set; }

        public string? FechaOrdenReferencia { get; set; }

        public string Moneda { get; set; }

        public string PlazoPago { get; set; }

        public decimal TotalPrecio { get; set; }

        public int TotalRegistros { get; set; }

        public decimal TotalIva { get; set; }

        public decimal TotalDescuento { get; set; }

        public decimal TotalVenta { get; set; }

        public decimal TotalBaseIva { get; set; }

        public decimal TotalReteIva { get; set; }

        public decimal TotalReteRenta { get; set; }

        public decimal TotalReteIca { get; set; }

        public string NotaResolucion { get; set; }

        public string NotaTipoPersona { get; set; }

        public string NotaRegimen { get; set; }

        public string NotaTipoContribuyente { get; set; }

        public string NotaAutorretendor { get; set; }
        public string DocumentoVenta { get; set; }

        public string NotaFe1 { get; set; }

        public string NotaFe2 { get; set; }
        public string? CodigoQR { get; set; }
        public int TypeEnvironmentId { get; set; }
        public string? TotalVentaLetras { get; set; }
        public string? notaFacturador { get; set; }
        public string? notaDireccion { get; set; }
        public string? SubjectEmail { get; set; }
        public string? observaciones1 { get; set; }
        public string? observaciones2 { get; set; }
        public string? codicionesGenerales1 { get; set; }
        public string? codicionesGenerales2 { get; set; }
        public string? xmlBase64 { get; set; }
        public List<PrintItem> Items { get; set; }
    }

}
