namespace UKMCAB.Common.Helpers
{
    public static class EnumerableHelper
    {
        public static bool AreListsEqual<T>(List<T> list1, List<T> list2) where T : IEquatable<T>
        {
            if (list1 == null && list2 == null)
                return true;
            if (list1 == null || list2 == null)
                return false;
            if (list1.Count != list2.Count)
                return false;

            var sortedList1 = list1.OrderBy(x => x);
            var sortedList2 = list2.OrderBy(x => x);

            return sortedList1.SequenceEqual(sortedList2);
        }

        public static bool AreObjectListsEqual<T>(List<T> list1, List<T> list2) where T : IEquatable<T>
        {
            if (list1 == null && list2 == null)
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
