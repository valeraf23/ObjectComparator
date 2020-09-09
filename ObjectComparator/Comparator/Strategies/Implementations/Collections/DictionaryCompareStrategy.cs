using System;
using System.Collections.Generic;
using System.Reflection;
using ObjectsComparator.Comparator.RepresentationDistinction;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections
{
    public class DictionaryCompareStrategy : BaseCollectionsCompareStrategy
    {
        private static readonly MethodInfo CompareMethod = typeof(DictionaryCompareStrategy).GetTypeInfo()
            .GetDeclaredMethod(nameof(CompareDictionary));

        public Distinctions CompareDictionary<TKey, TValue>(Dictionary<TKey, TValue> expected,
            Dictionary<TKey, TValue> actual, string propertyName)
        {
            if (expected.Count != actual.Count)
                return Distinctions.Create("Dictionary has different length", expected.Count, actual.Count);
            var diff = Distinctions.Create();
            foreach (var (key, value) in expected)
            {
                if (!actual.TryGetValue(key, out var secondValue))
                    diff.Add(new Distinction(key.ToString(), "Should be", "Does not exist"));

                var diffRes = Comparator.Compare(value, secondValue, $"{propertyName}[{key}]");
                diff.AddRange(diffRes);
            }

            return diff;
        }

        public override Distinctions Compare<T>(T expected, T actual, string propertyName)
        {
            var genericArguments = expected.GetType().GetGenericArguments();
            return (Distinctions) CompareMethod.MakeGenericMethod(genericArguments)
                .Invoke(this, new[] {(object) expected, actual, propertyName});
        }

        public override bool IsValid(Type member) =>
            member.IsGenericType && member.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }
}