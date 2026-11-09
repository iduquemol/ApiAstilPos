#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class ParametroNomina
    {
        public long IdParametroNomina { get; set; }
        public decimal? SalarioMinimoLegalVigente { get; set; }
        public decimal? AuxilioTransporteMensual { get; set; }
        public decimal? PorcentajeProvisionVacaciones { get; set; }
        public decimal? PorcentajeProvisionesCesantias { get; set; }
        public decimal? PorcentajeProvisionInteresesCesantias { get; set; }
        public decimal? PorcentajeProvisionesPrimas { get; set; }
        public decimal? PorcentajeAporteSalud { get; set; }
        public decimal? PorcentajeAportePension { get; set; }
        public decimal? PorcentajeFondoSolidaridad { get; set; }
        public decimal? PorcentajeFondoSubsistencia { get; set; }
        public decimal? HorasLaboralesMes { get; set; }
        public decimal? DiasVacacionesAno { get; set; }
        public DateTime? FechaInicialCesantias { get; set; }
        public DateTime? FechaFinalCesantias { get; set; }
        public DateTime? FechaInicialPrima { get; set; }
        public DateTime? FechaFinalPrima { get; set; }
        public DateTime? FechaPagoInteresesCesantias { get; set; }
        public DateTime? FechaConsignacionCesantias { get; set; }
        public DateTime? FechaPagoPrima { get; set; }
        public decimal? ValorUVT { get; set; }
        public byte? CantidadMinimoIntegral { get; set; }
        public decimal? PorcentajePrestacionIntegral { get; set; }
    }
}