namespace UKMCAB.Core.Domain.LegislativeAreas;

public class ScopeOfAppointmentOptionsModel
{
    public IEnumerable<PurposeOfAppointmentModel> PurposeOfAppointments { get; set; }

    public IEnumerable<CategoryModel> Categories { get; set; }

    public IEnumerable<SubCategoryModel> Subcategories { get; set; }

    public IEnumerable<ProductModel> Products { get; set; }

    public IEnumerable<ProcedureModel> Procedures { get; set; }
}
