using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Data.Models
{
    public class DocumentScopeOfAppointment
    {
        public Guid Id { get; set; }
        public Guid LegislativeAreaId { get; set; }

        public Guid? PurposeOfAppointmentId { get; set; }

        public Guid? CategoryId { get; set; }

        public Guid? SubCategoryId { get; set; }

        public List<Guid> ProductIds { get; set; } = new();

        public List<ProductAndProcedures> ProductIdAndProcedureIds { get; set; } = new();
        public List<Guid> ProcedureIds { get; set; } = new();
    }
}
