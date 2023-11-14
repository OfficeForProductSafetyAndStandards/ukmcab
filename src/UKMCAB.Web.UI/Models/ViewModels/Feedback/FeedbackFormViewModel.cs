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

        [RegularExpression("^([a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,})$", ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
        [MaxLength(50, ErrorMessage = "Maximum email address length is 50 characters")]
        public string? Email { get; set; }
        public string? ReturnURL { get; set; }
    }
}
