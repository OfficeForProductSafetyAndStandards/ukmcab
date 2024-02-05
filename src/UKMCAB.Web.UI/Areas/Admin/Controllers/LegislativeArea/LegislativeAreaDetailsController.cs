using Microsoft.AspNetCore.Authorization;
using UKMCAB.Web.UI.Models.ViewModels.Admin.CAB.LegislativeArea;

namespace UKMCAB.Web.UI.Areas.Admin.Controllers.LegislativeArea;


[Area("admin"), Route("admin/legislative-area"), Authorize]
public class LegislativeAreaDetailsController : Controller
{
    public static class Routes
    {
        public const string LegislativeAreaDetails = "legislative.area.details";
        public const string LegislativeAreaSelected = "legislative.area.selected";
        
    }
    public LegislativeAreaDetailsController()
    {
         
    }
    
    [HttpGet("details", Name = Routes.LegislativeAreaDetails)]
    public async Task<IActionResult> Details()
    {
        var vm = new LegislativeAreaDetailViewModel(Title: "Legislative area details");
        
        return View("~/Areas/Admin/views/CAB/LegislativeArea/Details.cshtml", vm);
    }
    
    [HttpGet("selected-legislative-area", Name = Routes.LegislativeAreaSelected)]
    public async Task<IActionResult> SelectedLegislativeArea()
    {
        var vm = new SelectedLegislativeAreasViewModel() 
        { 
            SelectedLegislativeAreas = new[]
            {
                new SelectedLegislativeAreaViewModel
                {
                    LegislativeAreaName = "Non-automatic weighting instruments",
                    LegislativeAreaDetails = new List<LegislativeAreaDetails> 
                    { new LegislativeAreaDetails 
                        { 
                            PurposeOfAppointment = "",
                            Category = "MI-005 Measuring systems for the continuous and dynamic measurement of quantities of liquid other than water",
                            SubCategory = "",
                            Product = "Measuring systems on a pipelines (Accuracy Class 0.3)",
                            Procedure = "Module G Conformity based on unit verification"
                        },
                        new LegislativeAreaDetails
                        {
                            PurposeOfAppointment = "",
                            Category = "MI-006 Automatic weighing machines",
                            SubCategory = "Automatic catch weigher",
                            Product = "Automatic catch weigher",
                            Procedure = "Module D1 Quality assurance of the production process"
                        }
                    }
                },
                new SelectedLegislativeAreaViewModel
                {
                    LegislativeAreaName = "Pressure equipment",
                    LegislativeAreaDetails = new List<LegislativeAreaDetails>
                    { new LegislativeAreaDetails
                        {
                            PurposeOfAppointment = "Conformity assessment of Pressure Equipment falling within Regulation 6 and classified in accordance with Schedule 3 as either Category I, II, III, or IV equipment",
                            Category = "Category II",
                            SubCategory = "",
                            Product = "Lorem ipsum dolor siture",
                            Procedure = "Part 2 – Module A2 Internal production control plus supervised pressure equipment checks at random"
                        },
                        new LegislativeAreaDetails
                        {
                            PurposeOfAppointment = "Not applicable",
                            Category = "Lorem ipsum dolor siture",
                            SubCategory = "",
                            Product = "Not applicable",
                            Procedure = "Part 2 – Module A2 Internal production control plus supervised pressure equipment checks at random"
                        }
                    }
                }

            }
        };
        
        return View("~/Areas/Admin/views/CAB/LegislativeArea/SelectedLegislativeArea.cshtml", vm);
    }
}