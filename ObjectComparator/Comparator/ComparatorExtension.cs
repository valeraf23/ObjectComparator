using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectComparator.Comparator.StrategiesForCertainProperties;
using ObjectComparator.Helpers.Extensions;

namespace ObjectComparator.Comparator
{
    public static class ComparatorExtension
    {
        public static DistinctionsCollection GetDifferenceBetweenObjects<T>(this T valueA, T valueB,
            params string[] ignore)
            where T : class => BaseGetDifferenceBetweenObjects(valueA, valueB, null, null, ignore);

        public static DistinctionsCollection GetDifferenceBetweenObjects<T>(this T valueA, T valueB,
            StrategiesCertainProperties<T> custom,
            params string[] ignore)
            where T : class => BaseGetDifferenceBetweenObjects(valueA, valueB, custom.ToArray(), null, ignore);

        public static DistinctionsCollection GetDifferenceBetweenObjects<T>(this T valueA, T valueB,
            Func<StrategiesCertainProperties<T>, IEnumerable<IMemberStrategy>> strategies,
            params string[] ignore)
            where T : class
        {
            var customStr = strategies(new StrategiesCertainProperties<T>());
            return BaseGetDifferenceBetweenObjects(valueA, valueB, customStr.ToArray(), null, ignore);
        }

        public static DistinctionsCollection BaseGetDifferenceBetweenObjects<T>(T objectA, T objectB,
            IList<IMemberStrategy> custom, string propertyName, IList<string> ignore)
            where T : class
        {
            if (objectA != null && objectB == null || objectA == null && objectB != null)
            {
                return new DistinctionsCollection {new Distinction(typeof(T).Name, objectA, objectB)};
            }

            var diff = new DistinctionsCollection();
            if (ReferenceEquals(objectA, objectB)) return diff;

            var type = objectA.GetType();

            var compareTypes = new CompareTypes();
            var ignoreList = SetIgnoreList(ignore, compareTypes);
            SetStrategies(custom, compareTypes);
            foreach (var mi in type.GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(x =>
                    x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field)
                .Where(m => !ignoreList.Contains(m.Name)))
            {
                object valueA = null;
                object valueB = null;

                var name = mi.Name;
                var actualPropertyPath = MemberPathBuilder.BuildMemberPath(propertyName, mi);

                switch (mi.MemberType)
                {
                    case MemberTypes.Field:
                        valueA = type.GetField(name).GetValue(objectA);
                        valueB = type.GetField(mi.Name).GetValue(objectB);
                        break;
                    case MemberTypes.Property:
                        valueA = type.GetProperty(name).GetValue(objectA);
                        valueB = type.GetProperty(name).GetValue(objectB);
                        break;
                }

                var diffRes = Compare(compareTypes, custom, actualPropertyPath, valueA, valueB);
                if (diffRes.IsNotEmpty())
                {
                    diff.AddRange(diffRes);
                }
            }

            return diff;
        }

        private static void SetStrategies(IList<IMemberStrategy> custom, CompareTypes compareTypes)
        {
            if (custom.IsNotEmpty())
            {
                compareTypes.SetStrategies(custom.ToList());
            }
        }

        private static List<string> SetIgnoreList(IList<string> ignore, CompareTypes compareTypes)
        {
            var ignoreList = new List<string>();
            if (!ignore.IsNotEmpty()) return ignoreList;
            compareTypes.SetIgnore(ignore);
            ignoreList = new List<string>(ignore);

            return ignoreList;
        }

        private static DistinctionsCollection Compare(CompareTypes comparesTypes,
            IList<IMemberStrategy> custom, string propertyName, dynamic valueA, dynamic valueB)
        {
            bool Predicate(IMemberStrategy strategy) => strategy.MemberName == propertyName;
            if (custom.IsEmpty() || !custom.Any(Predicate))
            {
                var diff = new DistinctionsCollection();
                if (valueA == null && valueB != null)
                {
                    return diff.Add(new Distinction(propertyName, "null", valueB));
                }

                if (valueA != null && valueB == null)
                {
                    return diff.Add(new Distinction(propertyName, valueA, "null"));
                }

                return valueA == null
                    ? diff
                    : (DistinctionsCollection) comparesTypes.Compare(valueA, valueB, propertyName);
            }

            var customStrategy = custom.First(Predicate);
            return customStrategy.Compare((object) valueA, (object) valueB, propertyName);
        }
    }
}