using UKMCAB.Data.Models.LegislativeAreas;
using static UKMCAB.Common.Helpers.EnumerableHelper;

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

        public List<ProductAndProcedures> ProductIdAndProcedureIds { get; set; } = new();
        public List<CategoryAndProcedures> CategoryIdAndProcedureIds { get; set; } = new();

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
                SubCategoryId != otherSoa.SubCategoryId)
            {
                return false;
            }

            if (!AreListsEqual(CategoryIds, otherSoa.CategoryIds) || !AreListsEqual(ProductIds, otherSoa.ProductIds))
                return false;

            if (!AreObjectListsEqual(CategoryIdAndProcedureIds, otherSoa.CategoryIdAndProcedureIds) ||
                !AreObjectListsEqual(ProductIdAndProcedureIds, otherSoa.ProductIdAndProcedureIds))
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
