﻿using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea
{
    public class LegislativeAreaViewModel : LegislativeAreaBaseViewModel
    {
        [Required(ErrorMessage = "Select a legislative area")]
        public Guid SelectedLegislativeAreaId { get; set; }

        public IEnumerable<SelectListItem>? LegislativeAreas { get; set; }
        public bool IsMRA { get; set; }
        public bool MRABypass { get; set; }

        public LegislativeAreaViewModel() : this("Create a CAB") { }
        public LegislativeAreaViewModel(string subTitle ) : base("Legislative area", subTitle) { }
    }
}
