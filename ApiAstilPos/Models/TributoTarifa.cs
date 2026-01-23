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
        public string CodigoTributo { get; set; } = string.Empty;
        public string NombreTributo { get; set; } = string.Empty;
        public TarifasTributo[] TarifasTributo { get; set; } = Array.Empty<TarifasTributo>();
    }
    public class TarifasTributo
    {
        public long IdTarifaTributo { get; set; }
        public string? CodigoTarifa { get; set; }
        public string? NombreTarifa { get; set; }
        public decimal TarifaTributo { get; set; }

    }
}
