using UKMCAB.Web.UI.Models;

namespace UKMCAB.Web.UI.Services;

public interface ISearchFilterService
{
    List<FilterOptionn> BodyTypeFilter { get; }
    List<FilterOptionn> RegisteredOfficeLocationFilter { get; }
    List<FilterOptionn> TestingLocationFilter { get; }
    List<FilterOptionn> RegulationFilter { get; }
}