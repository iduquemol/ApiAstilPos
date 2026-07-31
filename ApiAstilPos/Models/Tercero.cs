#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class Tercero
    {
        public long IdTercero { get; set; }
        public short IdTipoDocumentoId { get; set; }
        public string DigitoVerificacion { get; set; }
        public string NumeroIdentificacion { get; set; }
        public string PrimerNombre { get; set; }
        public string? SegundoNombre { get; set; }
        public string PrimerApellido { get; set; }
        public string? SegundoApellido { get; set; }
        public string RazonSocial { get; set; }
        public string? TelefonoTercero { get; set; }
        public string DireccionTercero { get; set; }
        public long IdMunicipio { get; set; }
        public string NombreMunicipio { get; set; }
        public string EmailTercero { get; set; }
        public long? IdTipoPersona { get; set; }
        public string NombreTipoPersona { get; set; }
        public long? IdDepartamento { get; set; }
        public string NombreDepartamento { get; set; }
        public bool? TerceroActivo { get; set; }
        public bool? TerceroCliente { get; set; }
        public bool? TerceroProveedor { get; set; }
        public bool? TercerosEmpleado { get; set; }
        public bool? TerceroGeneral { get; set; }
        public long? IdTipoRegimen { get; set; }
        public string NombreTipoRegimen { get; set; }
        public long? IdListaPreciosTercero { get; set; }

        // Mapeo de retenciones/tributos (igual que en ITercero.ts)
        public bool? RetenedorIva { get; set; }
        public bool? RetenedorRenta { get; set; }
        public bool? RetenedorIca { get; set; }
        public bool? DeclaraRenta { get; set; }
        public decimal? TarifaIca { get; set; }

        // Los dos campos nuevos
        public long? IdCodigoPostal { get; set; }
        public string? RegistroMercantil { get; set; }

        public ResponsabilidadTercero[] ResponsabilidadesTerceros { get; set; }
    }
}