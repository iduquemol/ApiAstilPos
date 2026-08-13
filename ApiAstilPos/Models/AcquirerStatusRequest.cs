namespace ApiAstilPos.Models
{
    public class AcquirerStatusRequest
    {
        public AcquirerStatusEnvironment environment { get; set; }
        public int type_document_identification_id { get; set; }
        public string identification_number { get; set; }
    }

    public class AcquirerStatusEnvironment
    {
        public int type_environment_id { get; set; }
    }
}
