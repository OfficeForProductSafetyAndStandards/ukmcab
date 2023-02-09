using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Feedback
{
    public class FeedbackFormViewModel : ILayoutModel
    {
        public string? Title => "Feedback from";

        [Required(ErrorMessage = "Please enter details of what you were doing")]
        public string? WhatWereYouDoing { get; set; }
        [Required(ErrorMessage = "Please enter details of what went wrong")]
        public string? WhatWentWrong { get; set; }
        public string? ReturnURL { get; set; }
    }
}
