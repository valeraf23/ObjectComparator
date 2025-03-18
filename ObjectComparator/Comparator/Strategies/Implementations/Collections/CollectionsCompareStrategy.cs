using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Rules;

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
            return CollectionHelper.GetDelegateFor(compareCollectionsMethod)(expected, actual,
                propertyName, RulesHandler);
        }

        private static Type GetGenericArgument(Type type)
        {
            return type.GetInterfaces().Prepend(type)
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == ListType)
                .Select(i => i.GetGenericArguments()[0]).First();
        }

        private static DeepEqualityResult CompareIListTypes<T>(IList<T> expected, IList<T> actual, string propertyName,
            RulesHandler rulesHandler)
        {
            var expectedCount = expected.Count;
            var actualCount = actual.Count;
            if (expectedCount != actualCount)
                return DistinctionsForCollectionsWithDifferentLength(propertyName, expected, actual);
            var diff = DeepEqualityResult.None();
            for (var i = 0; i < expectedCount; i++)
                diff.AddRange(rulesHandler.GetFor(typeof(T)).Compare(expected[i], actual[i], $"{propertyName}[{i}]"));

            return diff;
        }

        private static DeepEqualityResult CompareCollections<T>(IEnumerable<T> expected, IEnumerable<T> actual,
            string propertyName, RulesHandler rulesHandler)
        {
            var exp = expected.ToList();
            var act = actual.ToList();
            return CompareIListTypes(exp, act, propertyName, rulesHandler);
        }

        private static DeepEqualityResult DistinctionsForCollectionsWithDifferentLength<T>(string propertyName,
            IList<T> expected, IList<T> actual)
        {
            var itemType = typeof(T);

            if (itemType == typeof(string) || itemType.IsPrimitive || itemType.IsEnum)
            {
                var expectedList = expected.ToList();
                var actualList = actual.ToList();

                var diff = DeepEqualityResult.None();

                var addedItems = actualList.Except(expectedList).ToList();
                var removedItems = expectedList.Except(actualList).ToList();

                if (addedItems.Count > 0)
                {
                    diff.Add(new Distinction($"{propertyName}", null, string.Join(", ", addedItems), "Added"));
                }

                if (removedItems.Count > 0)
                {
                    diff.Add(new Distinction($"{propertyName}", string.Join(", ", removedItems), null,  "Removed"));
                }

                return diff;
            }

            return DeepEqualityResult.Create(new Distinction(
                $"Property \"{propertyName}\": Collection has different length", expected.Count, actual.Count));
        }
    }
}