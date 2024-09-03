using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABBodyDetailsMRAViewModel : CreateEditCABViewModel, ILayoutModel
    {
        public CABBodyDetailsMRAViewModel() { }

        public CABBodyDetailsMRAViewModel(Document document)
        {
            CABId = document.CABId;
            MRACountries = document.MRACountries ?? new List<string>();
            IsCompleted = MRACountries.Any();
        }

        public string? CABId { get; set; }

        [CannotBeEmpty(ErrorMessage = "Select a country")]
        public List<string> MRACountries { get; set; } = new();

        public string? Title => "Body MRA details";
        public string[] FieldOrder => new[] { nameof(MRACountries) };
    }
}
