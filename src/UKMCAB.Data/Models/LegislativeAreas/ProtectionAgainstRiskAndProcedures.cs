namespace UKMCAB.Data.Models.LegislativeAreas
{
    public class ProtectionAgainstRiskAndProcedures : IEquatable<ProtectionAgainstRiskAndProcedures>, IComparable<ProtectionAgainstRiskAndProcedures>
    {
        public Guid? ProtectionAgainstRiskId { get; set; }
        public List<Guid> ProcedureIds { get; set; } = new();

        public int CompareTo(ProtectionAgainstRiskAndProcedures? other)
        {
            if (other == null)
                return 1;

            if (ProtectionAgainstRiskId == null && other.ProtectionAgainstRiskId != null) 
                return 0;

            if(ProtectionAgainstRiskId == null) 
                return -1;

            if(other.ProtectionAgainstRiskId == null) 
                return 1;

            return ProtectionAgainstRiskId.Value.CompareTo(other.ProtectionAgainstRiskId);
        }

        public override bool Equals(object other)
        {
            return Equals(other as ProtectionAgainstRiskAndProcedures);
        }

        public bool Equals(ProtectionAgainstRiskAndProcedures other)
        {
            if (other == null) 
                return false;

            if (ProtectionAgainstRiskId != other.ProtectionAgainstRiskId)
                return false;

            return AreListsEqual(ProcedureIds, other.ProcedureIds);
 
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 31 + ProtectionAgainstRiskId.GetHashCode();
            
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
            if(list1 == null && list2 == null) 
                return true;

            if (!list1.Any() && !list2.Any())
                return true;

            if (list1 == null || list2 == null)
                return false;

            if(list1.Count != list2.Count)
                return false;

            var sortedList1 = list1.OrderBy(x => x).ToList();   
            var sortedList2 = list2.OrderBy(x => x).ToList();

            return sortedList1.SequenceEqual(sortedList2);
        }
    }
}
