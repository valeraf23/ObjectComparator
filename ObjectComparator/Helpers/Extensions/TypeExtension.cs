using System.Collections.Generic;

namespace ObjectsComparator.Helpers.Extensions
{
    public static class TypeExtension
    {
        public static bool IsDefault<T>(this T val)
        {
            return EqualityComparer<T>.Default.Equals(val, y: default);
        }

        public static bool IsNotDefault<T>(this T val)
        {
            return !IsDefault(val);
        }
    }
}