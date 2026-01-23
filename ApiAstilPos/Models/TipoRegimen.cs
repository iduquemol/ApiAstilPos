#nullable enable
using System;

namespace ApiAstilPos.Models
{
    public class TipoRegimen
    {
        public long IdTipoRegimen { get; set; }
        public string CodigoTipoRegimen { get; set; }
        public string NombreTipoRegimen { get; set; }
        public long? idTipoRegimenFe { get; set; }        
    }
}