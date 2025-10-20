using Newtonsoft.Json;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Rules;
using ObjectsComparator.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections
{
    public class CollectionsCompareStrategy : BaseCollectionsCompareStrategy
    {
        private static readonly MethodInfo CompareCollectionsMethod =
            typeof(CollectionsCompareStrategy).GetTypeInfo().GetDeclaredMethod(nameof(CompareCollections))!;

        private static readonly Type ListType = typeof(IEnumerable<>);

        public CollectionsCompareStrategy(Comparator comparator) : base(comparator)
        {
        }

        public override bool IsValid(Type member)
        {
            return member.GetInterfaces().Prepend(member).Any(interfaceType =>
                interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == ListType);
        }

        public override DeepEqualityResult Compare<T>(T expected, T actual, string propertyName)
        {
            var genericType = GetGenericArgument(expected?.GetType() ?? typeof(T));
            var compareCollectionsMethod = CompareCollectionsMethod.MakeGenericMethod(genericType);
            return CollectionHelper.GetDelegateFor(compareCollectionsMethod)(expected, actual, propertyName, RulesHandler);
        }

        private static Type GetGenericArgument(Type type)
        {
            return type.GetInterfaces().Prepend(type)
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == ListType)
                .Select(i => i.GetGenericArguments()[0]).First();
        }

        private static object GetFormattedValue<T>(T item, bool isPrimitive, bool couldToString)
        {
            return isPrimitive ? item : (couldToString ? item?.ToString() : FormatValue(item));
        }

        private static void AddExtraDifferences<T>(
            IReadOnlyList<T> source, 
            int startIndex, 
            string propertyName, 
            string action, 
            bool isPrimitive, 
            bool couldToString, 
            DeepEqualityResult diff)
        {
            for (var i = startIndex; i < source.Count; i++)
            {
                var value = GetFormattedValue(source[i], isPrimitive, couldToString);
                if (action == "Removed")
                {
                    diff.Add(new Distinction($"{propertyName}[{i}]", value, null, action));
                }
                else if (action == "Added")
                {
                    diff.Add(new Distinction($"{propertyName}[{i}]", null, value, action));
                }
            }
        }

        private static DeepEqualityResult CompareLists<T>(
            IReadOnlyList<T> expected, 
            IReadOnlyList<T> actual, 
            string propertyName, 
            RulesHandler rulesHandler)
        {
            var type = typeof(T);
            var isPrimitive = type == typeof(string) || type.IsPrimitive;
            var couldToString = type.IsEnum || type.IsToStringOverridden();
            var diff = DeepEqualityResult.None();
            var minCount = Math.Min(expected.Count, actual.Count);
            var handler = rulesHandler.GetFor(type);

            for (var i = 0; i < minCount; i++)
            {
                var elementDiff = handler.Compare(expected[i], actual[i], $"{propertyName}[{i}]");
                diff.AddRange(elementDiff);
            }

            if (expected.Count > actual.Count)
            {
                AddExtraDifferences(expected, actual.Count, propertyName, "Removed", isPrimitive, couldToString, diff);
            }
            else if (actual.Count > expected.Count)
            {
                AddExtraDifferences(actual, expected.Count, propertyName, "Added", isPrimitive, couldToString, diff);
            }

            return diff;
        }

        private static string FormatValue<T>(T value)
        {
            return JsonConvert.SerializeObject(value, SerializerSettings.Settings);
        }

        private static DeepEqualityResult CompareCollections<T>(
            IEnumerable<T> expected, 
            IEnumerable<T> actual, 
            string propertyName, 
            RulesHandler rulesHandler)
        {
            var exp = EnsureReadOnlyList(expected);
            var act = EnsureReadOnlyList(actual);
            return CompareLists(exp, act, propertyName, rulesHandler);
        }

        private static IReadOnlyList<T> EnsureReadOnlyList<T>(IEnumerable<T> source)
        {
            if (source is null)
            {
                return Array.Empty<T>();
            }

            switch (source)
            {
                case IReadOnlyList<T> readOnlyList:
                    return readOnlyList;
                case ICollection<T> collection:
                {
                    var array = new T[collection.Count];
                    collection.CopyTo(array, 0);
                    return array;
                }
                default:
                    return source.ToList();
            }
        }
    }
}
