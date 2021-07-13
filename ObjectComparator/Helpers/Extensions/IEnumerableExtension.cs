using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectsComparator.Helpers.Extensions
{
    public static class EnumerableExtension
    {
        public static bool IsEmpty<T>(this IEnumerable<T> collection) => collection is null || !collection.Any();

        public static bool IsNotEmpty<T>(this IEnumerable<T> collection) => !IsEmpty(collection);

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source) action(item);
        }
    }
}