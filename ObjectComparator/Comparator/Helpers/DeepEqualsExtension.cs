using System;
using System.Collections.Generic;
using System.Linq;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties;
using ObjectsComparator.Helpers.Extensions;

namespace ObjectsComparator.Comparator.Helpers
{
    public static class DeepEqualsExtension
    {

        public static DeepEqualityResult DeeplyEquals<T>(this T expected, T actual, params string[] ignore) =>
            DeeplyEquals(expected, actual, new Strategies<T>(), ignore);

        public static DeepEqualityResult DeeplyEquals<T>(this T expected, T actual, Strategies<T> custom, params string[] ignore)
        {
            var ignoreFullPath = ConvertToFullPath(ignore, typeof(T).ToFriendlyTypeName());
            return DeeplyEquals(expected, actual, custom.ToDictionary(x => x.Key, x => x.Value),
                ignoreFullPath.Contains);
        }

        public static DeepEqualityResult DeeplyEquals<T>(this T expected, T actual, Func<Strategies<T>, IEnumerable<KeyValuePair<string, ICustomCompareValues>>> strategies, params string[] ignore)
        {
            var ignoreFullPath = ConvertToFullPath(ignore, typeof(T).ToFriendlyTypeName());
            var customStr = strategies(new Strategies<T>());
            return DeeplyEquals(expected, actual, customStr.ToDictionary(x => x.Key, x => x.Value), ignoreFullPath.Contains);
        }

        public static DeepEqualityResult DeeplyEquals<T>(this T expected, T actual, Func<string, bool> ignoreStrategy) => DeeplyEquals(expected, actual, new Dictionary<string, ICustomCompareValues>(), ignoreStrategy);

        public static DeepEqualityResult DeeplyEquals<T>(T expected, T actual, Dictionary<string, ICustomCompareValues> custom, Func<string, bool> ignoreStrategy)
        {
            var compareTypes = new Comparator(custom, ignoreStrategy);
            return DeeplyEquals(expected, actual, compareTypes);
        }

        private static DeepEqualityResult DeeplyEquals<T>(T expected, T actual, Comparator compareObject) => compareObject.Compare(expected, actual);

        private static IEnumerable<string> ConvertToFullPath(IEnumerable<string> ignore, string typeName)
        {
            var enumerable = ignore.ToList();
            var ignoreFullPath = new List<string>(enumerable.Count);
            typeName = $"{typeName}.";
            foreach (var i in enumerable)
            {
                if (i.StartsWith(typeName))
                {
                    ignoreFullPath.Add(i);
                }

                ignoreFullPath.Add($"{typeName}{i}");
            }

            return ignoreFullPath;
        }
    }
}