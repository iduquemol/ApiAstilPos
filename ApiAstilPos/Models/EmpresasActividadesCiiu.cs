namespace ApiAstilPos.Models
{
    public class EmpresasActividadesCiiu
    {
        public long IdEmpresaActividadCiiu { get; set; }
        public long IdEmpresa { get; set; }
        public long IdActividadCiiu { get; set; }
        public bool EsActividadPrincipal { get; set; }
        public DateTime? FechaGrabacionEmpresaActividad { get; set; }
    }
}
