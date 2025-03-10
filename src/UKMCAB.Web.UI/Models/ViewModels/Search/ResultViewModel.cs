﻿using UKMCAB.Common.Extensions;
using UKMCAB.Data.Models;
using UKMCAB.Data.Search.Models;

namespace UKMCAB.Web.UI.Models.ViewModels.Search
{
    public class ResultViewModel
    {
        public ResultViewModel(CABIndexItem cab)
        {
            Name = cab.Name;
            Status = cab.Status;
            SubStatusName = ((SubStatus)int.Parse(cab.SubStatus)).GetEnumDescription();
            CABId = cab.CABId;
            URLSlug = cab.URLSlug;
            Address = StringExt.Join(", ", cab.AddressLine1, cab.AddressLine2, cab.TownCity, cab.County, cab.Postcode, cab.Country);
            BodyType = ListItems(cab.BodyTypes);
            MRACountry = ListItems(cab.MRACountries);
            RegisteredOfficeLocation = cab.RegisteredOfficeLocation;
            RegisteredTestLocation = ListItems(cab.TestingLocations);
            LegislativeArea = ListItems(cab.DocumentLegislativeAreas.Select(l => l.LegislativeAreaName));
            UserGroup = cab.CreatedByUserGroup;
        }

        private string? ListItems(IEnumerable<string> list)
        {
            if (list != null && list.Any())
            {
                return string.Join(", ", list.Select(l => l.Trim()));
            }
            return string.Empty;
        }

        public string? Name { get; set; }
        public string Status { get; set; }
        public string SubStatusName { get; set; }
        public string CABId { get; set; }
        public string URLSlug { get; set; }
        public string? Address { get; set; }
        public string? BodyType { get; set; }
        public string? MRACountry { get; set; }
        public string? RegisteredOfficeLocation { get; set; }
        public string? RegisteredTestLocation { get; set; }
        public string? LegislativeArea { get; set; }
        public string UserGroup { get;}
    }
}
