using System.Collections.Generic;
using System.Linq;

namespace ObjectsComparator.Helpers.Extensions
{
    public static class EnumerableExtension
    {      
        public static bool IsEmpty<T>(this IEnumerable<T> collection) => collection == null || !collection.Any();

        public static bool IsNotEmpty<T>(this IEnumerable<T> collection) => !IsEmpty(collection);

    }
}
