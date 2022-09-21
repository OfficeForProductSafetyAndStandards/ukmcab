using UKMCAB.Web.UI.Models;

namespace UKMCAB.Web.UI.Services;

public interface ISearchFilterService
{
    List<FilterOption> BodyTypeFilter { get; }
    List<FilterOption> RegisteredOfficeLocationFilter { get; }
    List<FilterOption> TestingLocationFilter { get; }
    List<FilterOption> RegulationFilter { get; }
}