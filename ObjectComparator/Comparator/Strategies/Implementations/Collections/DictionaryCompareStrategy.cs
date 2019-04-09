using System;
using System.Collections.Generic;
using ObjectsComparator.Comparator.RepresentationDistinction;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections
{
    public class DictionaryCompareStrategy : BaseCollectionsCompareStrategy
    {
        public Distinctions CompareDictionary<TKey, TValue>(IDictionary<TKey, TValue> expected,
            IDictionary<TKey, TValue> actual, string propertyName)
        {
            var diff = new Distinctions();
            if (expected.Count != actual.Count)
                return Distinctions.Create("Dictionary has different length", expected.Count, actual.Count);
            foreach (var kvp in expected)
            {
                if (!actual.TryGetValue(kvp.Key, out var secondValue))
                    diff.Add(new Distinction(kvp.Key.ToString(), "Should be", "Does not exist"));

                var diffRes = Comparator.Compare(kvp.Value, secondValue, $"{propertyName}[{kvp.Key}]");
                diff.AddRange(diffRes);
            }

            return diff;
        }

        public override Distinctions Compare<T>(T expected, T actual, string propertyName)
        {
            var genericArguments = expected.GetType().GetGenericArguments();
            return (Distinctions) GetType().GetMethod("CompareDictionary").MakeGenericMethod(genericArguments)
                .Invoke(this, new[] {(object) expected, actual, propertyName});
        }

        public override bool IsValid(Type member) =>
            member.IsGenericType && member.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }
}