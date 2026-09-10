#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiAstilPos.Models
{
   public class TributoTarifa
    {
        public long IdTributo { get; set; }
        public string? CodigoTributo { get; set; }
        public string? NombreTributo { get; set; }
        public List<TarifaTributoDetail>? TarifasTributo { get; set; }
    }

    public class TarifaTributoDetail
    {
        public long IdTarifaTributo { get; set; }
        public long? IdTributo { get; set; }
        public string? CodigoTarifa { get; set; }
        public string? NombreTarifa { get; set; }
        public string? DescripcionTarifa { get; set; }
        public bool? AplicaDeclarante { get; set; }
        public bool? AplicaNoDeclarante { get; set; }
        public decimal? BaseUVT { get; set; }
        public decimal? TarifaTributo { get; set; }
        public bool? TarifaActiva { get; set; }
        public bool? TarifaDefault { get; set; }
        public string? IdExterno { get; set; }
    }
}
