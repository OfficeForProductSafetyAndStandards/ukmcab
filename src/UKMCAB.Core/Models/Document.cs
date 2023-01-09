namespace UKMCAB.Core.Models
{
    public class Document
    {
        public string id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public List<Audit> AuditHistory { get; set; }
        public CABData CABData { get; set; }
        public State State { get; set; }
        public string StateAsString => State.ToString();
    }
}
