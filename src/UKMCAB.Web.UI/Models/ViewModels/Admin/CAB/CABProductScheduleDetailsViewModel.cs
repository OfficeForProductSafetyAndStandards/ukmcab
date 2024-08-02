using UKMCAB.Data.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB
{
    public class CABProductScheduleDetailsViewModel : CreateEditCABViewModel
    {
        public CABProductScheduleDetailsViewModel() { }

        public CABProductScheduleDetailsViewModel(Document document)
        {
            CABId = document.CABId;            
            ActiveSchedules = document.ActiveSchedules ?? new List<FileUpload>();
            ArchivedSchedules = document.ArchivedSchedules ?? new List<FileUpload>();
            IsCompleted = this.ProductSchedulesCompleted();
        }

        public List<FileUpload>? ActiveSchedules { get; set; }
        public List<FileUpload>? ArchivedSchedules { get; set; }

        public bool IsCompleted { get; set; }

        public string? CABId { get; set; }

        public bool ProductSchedulesCompleted()
        {
            if (this.ActiveSchedules != null && this.ActiveSchedules.Any())
            {
                // any file where file label is empty/null
                if (this.ActiveSchedules.Any(u => string.IsNullOrWhiteSpace(u.Label)))
                {
                    return false;
                }
                // any file where legislative area not selected
                else if (this.ActiveSchedules.Any(u => string.IsNullOrWhiteSpace(u.LegislativeArea)))
                {
                    return false;
                }
                // any duplicate file labels/legislative area
                else if(this.ActiveSchedules.Where(x => !string.IsNullOrWhiteSpace(x.LegislativeArea) && !string.IsNullOrWhiteSpace(x.Label)).GroupBy(x => new { Label = x.Label!.ToLower(), LegislativeArea = x.LegislativeArea!.ToLower() }).Any(g => g.Count() > 1))
                {
                    return false;
                }
                // any duplicate files/legislative area
                else if(this.ActiveSchedules.Where(x => !string.IsNullOrWhiteSpace(x.LegislativeArea)).GroupBy(x => new { FileName = x.FileName.ToLower(), LegislativeArea = x.LegislativeArea!.ToLower()}).Any(g => g.Count() > 1))
                {
                    return false;
                }
                else
                {
                    return true;
                }              
            }

            return true;
        }
    }
}
