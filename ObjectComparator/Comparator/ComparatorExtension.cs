using System;
using System.Collections.Generic;
using System.Linq;
using ObjectComparator.Comparator.Implementations;
using ObjectComparator.Comparator.Interfaces;
using ObjectComparator.Comparator.StrategiesForCertainProperties;

namespace ObjectComparator.Comparator
{
    public static class ComparatorExtension
    {
        public static DistinctionsCollection GetDifferenceBetweenObjects<T>(this T valueA, T valueB,
            params string[] ignore)
            where T : class => GetDifferenceBetweenObjects(valueA, valueB, null, null, ignore);

        public static DistinctionsCollection GetDifferenceBetweenObjects<T>(this T valueA, T valueB,
            Strategies<T> custom,
            params string[] ignore)
            where T : class => GetDifferenceBetweenObjects(valueA, valueB,
            custom.ToDictionary(x => x.Key, x => x.Value), null, ignore);

        public static DistinctionsCollection GetDifferenceBetweenObjects<T>(this T valueA, T valueB,
            Func<Strategies<T>, IEnumerable<KeyValuePair<string, ICompareValues>>> strategies,
            params string[] ignore)
            where T : class
        {
            var customStr = strategies(new Strategies<T>());
            return GetDifferenceBetweenObjects(valueA, valueB, customStr.ToDictionary(x => x.Key, x => x.Value),
                null, ignore);
        }

        public static DistinctionsCollection GetDifferenceBetweenObjects<T>(T objectA, T objectB,
            IDictionary<string, ICompareValues> custom, string propertyName, IList<string> ignore)
            where T : class
        {
            var compareTypes = new CompareObjectsStrategy();
            compareTypes.SetIgnore(ignore);
            compareTypes.SetStrategies(custom);
            return GetDifferenceBetweenObjects(objectA, objectB, compareTypes);
        }

        public static DistinctionsCollection GetDifferenceBetweenObjects<T>(T objectA, T objectB, ICompareObjectStrategy compareObject)
            where T : class => compareObject.Compare(objectA, objectB);
    }
}