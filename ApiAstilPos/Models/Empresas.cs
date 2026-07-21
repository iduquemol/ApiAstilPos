#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class empresas
    {
        public long idEmpresa { get; set; }
        public long idTipoDocumentoId { get; set; }
        public string? nombreTipoDocumentoId { get; set; }
        public string nitEmpresa { get; set; } = string.Empty;
        public string? digitoVerificacion { get; set; }
        public string nombreEmpresa { get; set; } = string.Empty;
        public string? nombreComercial { get; set; }
        public long idTipoPersona { get; set; }
        public string? nombreTipoPersona { get; set; }
        public string? idResponsabilidadFiscal { get; set; }
        public string? idResponsabilidadFiscal2 { get; set; }
        public string? idResponsabilidadFiscal3 { get; set; }
        public string? monedaEmpresa { get; set; }
        public long idTipoRegimen { get; set; }
        public string? nombreTipoRegimen { get; set; }
        public string? registroMercantil { get; set; }
        public string? direccionEmpresa { get; set; }
        public string? telefonoEmpresa { get; set; }
        public long idDepartamento { get; set; }
        public string? nombreDepartamento { get; set; }
        public long idMunicipio { get; set; }
        public string? nombreMunicipio { get; set; }
        public string? emailEmpresa { get; set; }
        public string? notaFe1 { get; set; }
        public string? notaFe2 { get; set; }
        public long? idTipoAsignacionResolucion { get; set; }
        public bool? habilitacionFacturacion { get; set; }
        public bool? responsableIva { get; set; }
        public bool? granContribuyente { get; set; }
        public bool? autoretenedor { get; set; }
        public bool? responsableImpoConsumo { get; set; }
        public bool? agenteRetenedorIva { get; set; }
        public bool? agenteRetenedorRenta { get; set; }
        public string? idRepresentanteLegal { get; set; }
        public string? correoElectronicoRepresentante { get; set; }
        public decimal? tarifaReteIca { get; set; }
        public decimal? tarifaReteIva { get; set; }
        public string? actividadEconomica { get; set; }
        public int? ambienteDian { get; set; }
        public DateTime? fechaGrabacionEmpresa { get; set; }
        public string? notaFe3 { get; set; }
        public long? idMedioPagoContado { get; set; }
        public string? nombreMedioPagoContado { get; set; }
        public long? idMedioPagoCredito { get; set; }
        public string? nombreMedioPagoCredito { get; set; }
    }
}