using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ApiAstilPos.Models
{
    public class RecepcionDetalle
    {
        public bool? Is_Valid { get; set; }
        public string? Uuid { get; set; }
        public string? Number { get; set; }
        public string? Issue_Date { get; set; }
        public string? Name { get; set; }
        public string? Identification_Number { get; set; }
        public string? Receiver_Name { get; set; }
        public string? Receiver_Identification_Number { get; set; }
        public string? Total { get; set; }
        public string? Subject { get; set; }
        public string? Status_Message { get; set; }
        public List<string>? Errors_Messages { get; set; }
        public Dictionary<string, JsonElement>? Attachments { get; set; }
    }
}