using UKMCAB.Data.Models.LegislativeAreas;
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
    public IEnumerable<PpeProductTypeModel> PpeProductType { get; set; } = Array.Empty<PpeProductTypeModel>();
    public IEnumerable<ProtectionAgainstRiskModel> ProtectionAgainstRisk { get; set; } = Array.Empty<ProtectionAgainstRiskModel>();
    public IEnumerable<AreaOfCompetencyModel> AreaOfCompetency { get; set; } = Array.Empty<AreaOfCompetencyModel>();
    public PaginationInfo? PaginationInfo { get; set; }
}
