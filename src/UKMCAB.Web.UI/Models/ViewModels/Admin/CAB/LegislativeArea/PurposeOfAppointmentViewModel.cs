﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class PurposeOfAppointmentViewModel : LegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select a purpose of appointment")]
        public Guid? SelectedPurposeOfAppointmentId { get; set; }
        
        public Guid CabId { get; set; }

        public string? LegislativeArea { get; set; } = string.Empty;

        public IEnumerable<SelectListItem>? PurposeOfAppointments { get; set; }

        public PurposeOfAppointmentViewModel() : this("Create a CAB") { }
        public PurposeOfAppointmentViewModel(string subTitle) : base("Legislative area purpose of appointment", subTitle) { }
    }
}
