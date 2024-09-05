using UKMCAB.Data.Models.LegislativeAreas;

namespace UKMCAB.Data.Models
{
    public class DocumentScopeOfAppointment : IEquatable<DocumentScopeOfAppointment>
    {
        public Guid Id { get; set; }
        public Guid LegislativeAreaId { get; set; }
        public Guid? PurposeOfAppointmentId { get; set; }
        public Guid? CategoryId { get; set; }
        public List<Guid> CategoryIds { get; set; } = new();
        public Guid? SubCategoryId { get; set; }
        public List<Guid> ProductIds { get; set; } = new();
        public List<Guid> AreaOfCompetencyIds { get; set; } = new();
        public List<ProductAndProcedures> ProductIdAndProcedureIds { get; set; } = new();
        public List<CategoryAndProcedures> CategoryIdAndProcedureIds { get; set; } = new();
        public List<AreaOfCompetencyAndProcedures> AreaOfCompetencyIdAndProcedureIds { get; set; } = new();
        public List<Guid> DesignatedStandardIds { get; set; } = new();
        public Guid? PpeProductTypeId { get; set; }
        public Guid? ProtectionAgainstRiskId { get; set; }

        public override bool Equals(object? scopeOfAppointment) 
        { 
            return Equals(scopeOfAppointment as DocumentScopeOfAppointment);
        }
        public bool Equals(DocumentScopeOfAppointment? otherSoa)
        {            
            if (otherSoa == null) 
                return false;

            if (!LegislativeAreaId.Equals(otherSoa.LegislativeAreaId) ||
                PurposeOfAppointmentId != otherSoa.PurposeOfAppointmentId ||
                CategoryId != otherSoa.CategoryId ||
                SubCategoryId != otherSoa.SubCategoryId ||
                PpeProductTypeId != otherSoa.PpeProductTypeId ||
                ProtectionAgainstRiskId != otherSoa.ProtectionAgainstRiskId)
            {
                return false;
            }

            if (!AreListsEqual(CategoryIds, otherSoa.CategoryIds) || 
                !AreListsEqual(ProductIds, otherSoa.ProductIds) || 
                !AreListsEqual(AreaOfCompetencyIds, otherSoa.AreaOfCompetencyIds))
                return false;

            if (!AreObjectListsEqual(CategoryIdAndProcedureIds, otherSoa.CategoryIdAndProcedureIds) ||
                !AreObjectListsEqual(ProductIdAndProcedureIds, otherSoa.ProductIdAndProcedureIds) ||
                !AreObjectListsEqual(AreaOfCompetencyIdAndProcedureIds, otherSoa.AreaOfCompetencyIdAndProcedureIds))
                return false;

            if (!AreListsEqual(DesignatedStandardIds, otherSoa.DesignatedStandardIds))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            var hash = 17;
            hash = hash * 31 + LegislativeAreaId.GetHashCode();
            hash = hash * 31 + PurposeOfAppointmentId.GetHashCode();
            hash = hash * 31 + CategoryId.GetHashCode();
            hash = hash * 31 + SubCategoryId.GetHashCode();
            hash = hash * 31 + PpeProductTypeId.GetHashCode();
            hash = hash * 31 + ProtectionAgainstRiskId.GetHashCode();

            if (CategoryIds.Any())
            {
                foreach (var catId in CategoryIds)
                {
                    hash = hash * 31 + catId.GetHashCode();
                }
            }

            if (ProductIds.Any())
            {
                foreach (var prodId in ProductIds)
                {
                    hash = hash * 31 + prodId.GetHashCode();
                }
            }

            if (AreaOfCompetencyIds.Any())
            {
                foreach (var areaOfCompId in AreaOfCompetencyIds)
                {
                    hash = hash * 31 + areaOfCompId.GetHashCode();
                }
            }

            if (ProductIdAndProcedureIds.Any())
            {
                foreach (var prodAndProcedureId in ProductIdAndProcedureIds)
                {
                    hash = hash * 31 + prodAndProcedureId.GetHashCode();
                }
            }

            if (CategoryIdAndProcedureIds.Any())
            {
                foreach (var catAndProcedureId in CategoryIdAndProcedureIds)
                {
                    hash = hash * 31 + catAndProcedureId.GetHashCode();
                }
            }

            if (AreaOfCompetencyIdAndProcedureIds.Any())
            {
                foreach (var areaOfCompetencyAndProcedureId in AreaOfCompetencyIdAndProcedureIds)
                {
                    hash = hash * 31 + areaOfCompetencyAndProcedureId.GetHashCode();
                }
            }

            if (DesignatedStandardIds.Any())
            {
                foreach (var designatedStandardId in DesignatedStandardIds)
                {
                    hash = hash * 31 + designatedStandardId.GetHashCode();
                }
            }
            return hash;
        }
        private bool AreListsEqual(List<Guid> list1, List<Guid> list2)
        {
            if (list1 == null && list2 == null)
                return true;
            if (list1 == null || list2 == null)
                return false;
            if (list1.Count != list2.Count)
                return false;

            var groupedList1 = list1.GroupBy(x => x).OrderBy(g => g.Key);
            var groupedList2 = list2.GroupBy(x => x).OrderBy(g => g.Key);

            return groupedList1.SequenceEqual(groupedList2, new GroupComparer());
        }

        private bool AreObjectListsEqual<T>(List<T> list1, List<T> list2) where T : IEquatable<T> 
        { 
            if(list1 == null && list2 == null)
                return true;
            if(list1 == null || list2 == null)
                return false;
            if(list1.Count != list2.Count)
                return false;

            var sortedList1 = list1.OrderBy(x => x).ToList();
            var sortedList2 = list2.OrderBy(x => x).ToList();

            return sortedList1.SequenceEqual(sortedList2);
        }        
    }

    public class GroupComparer : IEqualityComparer<IGrouping<Guid, Guid>>
    {
        public bool Equals(IGrouping<Guid, Guid> x, IGrouping<Guid, Guid> y)
        {
            return x.Key == y.Key && x.Count() == y.Count();    
        }

        public int GetHashCode(IGrouping<Guid, Guid> obj)
        {
            return obj.Key.GetHashCode() ^ obj.Count().GetHashCode();
        }
    }
}
