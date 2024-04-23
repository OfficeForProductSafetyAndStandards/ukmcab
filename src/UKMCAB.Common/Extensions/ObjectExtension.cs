using Newtonsoft.Json;

namespace UKMCAB.Common.Extensions
{
    public static class ObjectExtension
    {
        public static T DeepCopy<T>(this T self)
        {
            var serialized = JsonConvert.SerializeObject(self);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
