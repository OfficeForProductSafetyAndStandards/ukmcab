namespace UKMCAB.Data.Models
{
    public class DocumentLegislativeArea
    {
        public Guid Id { get; set; }
        public Guid LegislativeAreaId { get; set; }

        public DateTime? AppointmentDate { get; set; }

        public bool? IsProvisional { get; set; }

        public DateTime? ReviewDate { get; set; }

        public string? Reason { get; set; }

    }
}
