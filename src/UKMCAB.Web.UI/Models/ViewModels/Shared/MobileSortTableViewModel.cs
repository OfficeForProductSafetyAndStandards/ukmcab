namespace UKMCAB.Web.UI.Models.ViewModels.Shared;

public record MobileSortTableViewModel(string SortField, string SortDirection, IEnumerable<Tuple<string,string>> SortSelectItems);