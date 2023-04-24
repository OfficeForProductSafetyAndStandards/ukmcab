using System.ComponentModel.DataAnnotations;

namespace UKMCAB.Common
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CannotBeEmptyAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            var list = value as IEnumerable<string>;
            return list != null && list.Any(l => !string.IsNullOrWhiteSpace(l));
        }
    }
}
