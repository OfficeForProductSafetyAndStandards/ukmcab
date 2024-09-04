using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABBodyDetailsMRAViewModel : CABBodyDetailsViewModel, ILayoutModel
    {
        public CABBodyDetailsMRAViewModel() { }

        public CABBodyDetailsMRAViewModel(Document document) : base(document)
        {
            MRACountries = document.MRACountries ?? new List<string>();
            IsCompleted = TestingLocations.Any() && BodyTypes.Any() && (!isMRA || MRACountries.Any());
        }

        [CannotBeEmpty(ErrorMessage = "Select a country")]
        public List<string> MRACountries { get; set; } = new();

        public new string? Title => "Body MRA details";
        public new string[] FieldOrder => new[] { nameof(MRACountries) };
    }
}
