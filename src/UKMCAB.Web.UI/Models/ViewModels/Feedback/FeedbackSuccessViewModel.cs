namespace UKMCAB.Web.UI.Models.ViewModels.Feedback
{
    public class FeedbackSuccessViewModel : ILayoutModel
    {
        public string? Title => "Thank for your feedback";
        public string ReturnURL { get; set; }
    }
}
