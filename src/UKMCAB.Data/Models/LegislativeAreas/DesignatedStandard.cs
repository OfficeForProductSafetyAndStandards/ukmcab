﻿using UKMCAB.Data.Pagination;

namespace UKMCAB.Data.Models.LegislativeAreas
{
    public class DesignatedStandard : IOrderable
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid LegislativeAreaId { get; set; }
        public string Regulation { get; set; }
        public List<string> ReferenceNumber { get; set; }
        public string NoticeOfPublicationReference { get; set; }
    }
}
