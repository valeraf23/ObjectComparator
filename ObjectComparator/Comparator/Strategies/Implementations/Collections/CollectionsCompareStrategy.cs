using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectsComparator.Comparator.RepresentationDistinction;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections
{
    public class CollectionsCompareStrategy : BaseCollectionsCompareStrategy
    {
        private static readonly MethodInfo CompareCollectionsMethod =
            typeof(CollectionsCompareStrategy).GetTypeInfo().GetDeclaredMethod(nameof(CompareCollections));

        public override bool IsValid(Type member)
        {
            return member.GetInterfaces().Contains(typeof(IEnumerable)) && member != typeof(string);
        }

        public override Distinctions Compare<T>(T expected, T actual, string propertyName)
        {
            var elementType = expected.GetType();
            var genericType = elementType.IsGenericType
                ? elementType.GenericTypeArguments[0]
                : elementType.GetElementType();
            var compareCollectionsMethod = CompareCollectionsMethod.MakeGenericMethod(genericType!);
            return CollectionHelper.GetDelegateFor(compareCollectionsMethod)(expected, actual,
                propertyName, Comparator);
        }

        private static Distinctions CompareIListTypes<T>(IList<T> expected, IList<T> actual, string propertyName,
            Comparator comparator)
        {
            var expectedCount = expected.Count;
            var actualCount = actual.Count;
            if (expectedCount != actualCount)
                return DistinctionsForCollectionsWithDifferentLength(propertyName, expectedCount, actualCount);
            var diff = Distinctions.None();
            for (var i = 0; i < expectedCount; i++)
                diff.AddRange(comparator.GetDistinctions($"{propertyName}[{i}]", expected[i], actual[i]));

            return diff;
        }

        private static Distinctions CompareCollections<T>(IEnumerable<T> expected, IEnumerable<T> actual,
            string propertyName, Comparator comparator)
        {
            var a = expected.ToList();
            var b = actual.ToList();
            return CompareIListTypes(a, b, propertyName, comparator);
        }

        private static Distinctions DistinctionsForCollectionsWithDifferentLength(string propertyName, int first,
            int second)
        {
            return Distinctions.Create(new Distinction(
                $"Property \"{propertyName}\": Collection has different length", $"{second}",
                $"{first}"));
        }
    }
}