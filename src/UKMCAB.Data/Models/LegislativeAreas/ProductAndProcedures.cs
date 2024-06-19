namespace UKMCAB.Data.Models.LegislativeAreas
{
    public class ProductAndProcedures : IEquatable<ProductAndProcedures>
    {
        public Guid? ProductId { get; set; }
        public List<Guid> ProcedureIds { get; set; } = new();

        public override bool Equals(object otherCatProc)
        {
            return Equals(otherCatProc as ProductAndProcedures);
        }

        public bool Equals(ProductAndProcedures otherProdAndProc)
        {
            if (otherProdAndProc == null)
                return false;

            if (!ProductId.Equals(otherProdAndProc.ProductId))
                return false;

            return AreListsEqual(ProcedureIds, otherProdAndProc.ProcedureIds);

        }
        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 31 + ProductId.GetHashCode();

            if (ProcedureIds.Any())
            {
                foreach (var procId in ProcedureIds)
                {
                    hash = hash * 31 + procId.GetHashCode();
                }
            }

            return hash;
        }
        private bool AreListsEqual(List<Guid> list1, List<Guid> list2)
        {
            if (list1 == null && list2 == null)
                return true;

            if (!list1.Any() && !list2.Any())
                return true;

            if (list1 == null || list2 == null)
                return false;

            if (list1.Count != list2.Count)
                return false;

            var sortedList1 = list1.OrderBy(x => x).ToList();
            var sortedList2 = list2.OrderBy(x => x).ToList();

            return sortedList1.SequenceEqual(sortedList2);
        }
    }
}
