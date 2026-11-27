using System;

namespace ApiAstilPos.Models
{
    public class RecepcionDocumento
    {
        public bool? Is_Valid { get; set; }
        public string? Uuid { get; set; }
        public string? Number { get; set; }
        public string? Issue_Date { get; set; }
        public string? Name { get; set; }
        public string? Identification_Number { get; set; }
        public string? Receiver_Name { get; set; }
        public string? Total { get; set; }
    }
}