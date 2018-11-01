using System;

namespace ObjectsComparator.Helpers.Extensions
{
    public static class ConvertExtension
    {
        public static T ChangeType<T>(this object o)
        {
            var conversionType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            return (T) Convert.ChangeType(o, conversionType);
        }
    }
}
