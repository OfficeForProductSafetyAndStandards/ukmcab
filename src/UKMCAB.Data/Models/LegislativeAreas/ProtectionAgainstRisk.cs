﻿namespace UKMCAB.Data.Models.LegislativeAreas
{
    public class ProtectionAgainstRisk : IEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Guid> PpeProductTypeIds { get; set; }
    }
}