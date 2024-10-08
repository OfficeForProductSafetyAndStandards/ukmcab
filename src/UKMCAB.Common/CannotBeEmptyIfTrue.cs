using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace UKMCAB.Common
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CannotBeEmptyIfTrueAttribute : RequiredAttribute
    {
        private string PropertyName { get; set; }

        public CannotBeEmptyIfTrueAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            object instance = validationContext.ObjectInstance;
            Type type = instance.GetType();

            if (!bool.TryParse(type.GetProperty(PropertyName).GetValue(instance)?.ToString(), out bool propertyValue))
            {
                return new ValidationResult("Could not find a property named '{0}'.");
            };
            
            var list = value as IEnumerable<string>;
            if(propertyValue && (list == null || !list.Any(l => !string.IsNullOrWhiteSpace(l))))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
