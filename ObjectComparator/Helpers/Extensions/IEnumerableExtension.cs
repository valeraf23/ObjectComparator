using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectsComparator.Helpers.Extensions;

public static class EnumerableExtension
{
    public static bool IsEmpty<T>(this IEnumerable<T> collection)
    {
        return collection is null || !collection.Any();
    }

    public static bool IsNotEmpty<T>(this IEnumerable<T> collection)
    {
        return !collection.IsEmpty();
    }

    public static bool IsEmpty<T>(this IList<T> collection)
    {
        return collection is null || collection.Count == 0;
    }

    public static bool IsNotEmpty<T>(this IList<T> collection)
    {
        return !collection.IsEmpty();
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var item in source)
        {
            action(item);
        }
    }
}