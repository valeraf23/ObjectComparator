using System;
using System.Collections;
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
            var genericType = GetGenericArgument(typeof(T));
            var compareCollectionsMethod = CompareCollectionsMethod.MakeGenericMethod(genericType!);
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
                return DistinctionsForCollectionsWithDifferentLength(propertyName, expectedCount, actualCount);
            var diff = DeepEqualityResult.None();
            for (var i = 0; i < expectedCount; i++)
                diff.AddRange(rulesHandler.GetFor(typeof(T)).Compare(expected[i], actual[i], $"{propertyName}[{i}]"));

            return diff;
        }

        private static DeepEqualityResult CompareCollections<T>(IEnumerable<T> expected, IEnumerable<T> actual, string propertyName, RulesHandler rulesHandler)
        {
            var exp = expected.ToList();
            var act = actual.ToList();
            return CompareIListTypes(exp, act, propertyName, rulesHandler);
        }

        private static DeepEqualityResult DistinctionsForCollectionsWithDifferentLength(string propertyName, int first, int second) =>
            DeepEqualityResult.Create(new Distinction($"Property \"{propertyName}\": Collection has different length", $"{second}", $"{first}"));
    }
}