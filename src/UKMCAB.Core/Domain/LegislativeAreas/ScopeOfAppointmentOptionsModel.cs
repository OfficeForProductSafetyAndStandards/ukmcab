using UKMCAB.Data.Pagination;

namespace UKMCAB.Core.Domain.LegislativeAreas;

public class ScopeOfAppointmentOptionsModel
{
    public IEnumerable<PurposeOfAppointmentModel> PurposeOfAppointments { get; set; } =
        Array.Empty<PurposeOfAppointmentModel>();

    public IEnumerable<CategoryModel> Categories { get; set; } =  Array.Empty<CategoryModel>();

    public IEnumerable<SubCategoryModel> Subcategories { get; set; } = Array.Empty<SubCategoryModel>();

    public IEnumerable<ProductModel> Products { get; set; } = Array.Empty<ProductModel>();

    public IEnumerable<ProcedureModel> Procedures { get; set; } = Array.Empty<ProcedureModel>();
    public IEnumerable<DesignatedStandardModel> DesignatedStandards { get; set; } = Array.Empty<DesignatedStandardModel>();
    public PaginationInfo? PaginationInfo { get; set; }
}
