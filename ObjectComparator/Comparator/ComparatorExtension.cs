using System;
using System.Collections.Generic;
using System.Linq;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties;

namespace ObjectsComparator.Comparator
{
    public static class ComparatorExtension
    {
        public static Distinctions GetDistinctions<T>(this T expected, T actual,
            params string[] ignore)
            where T : class
        {
            return GetDistinctions(expected, actual, new Strategies<T>(), ignore);
        }

        public static Distinctions GetDistinctions<T>(this T expected, T actual,
            Strategies<T> custom, params string[] ignore)
            where T : class
        {
            return GetDistinctions(expected, actual,
                custom.ToDictionary(x => x.Key, x => x.Value), ignore.Contains);
        }

        public static Distinctions GetDistinctions<T>(this T expected, T actual,
            Func<Strategies<T>, IEnumerable<KeyValuePair<string, ICompareValues>>> strategies,
            params string[] ignore)
            where T : class
        {
            var customStr = strategies(new Strategies<T>());
            return GetDistinctions(expected, actual, customStr.ToDictionary(x => x.Key, x => x.Value), ignore.Contains);
        }

        public static Distinctions GetDistinctions<T>(this T expected, T actual, Func<string, bool> ignoreStrategy)
            where T : class
        {
            return GetDistinctions(expected, actual, null, ignoreStrategy);
        }

        public static Distinctions GetDistinctions<T>(T expected, T actual,
            IDictionary<string, ICompareValues> custom, Func<string, bool> ignoreStrategy)
            where T : class
        {
            var compareTypes = new Comparator();
            compareTypes.SetIgnore(ignoreStrategy);
            compareTypes.SetStrategies(custom);
            return GetDistinctions(expected, actual, compareTypes);
        }

        public static Distinctions GetDistinctions<T>(T expected, T actual, Comparator compareObject)
            where T : class
        {
            return compareObject.Compare(expected, actual);
        }
    }
}