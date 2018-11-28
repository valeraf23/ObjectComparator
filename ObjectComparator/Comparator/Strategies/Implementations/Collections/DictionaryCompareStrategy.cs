using System;
using System.Collections.Generic;
using ObjectsComparator.Comparator.RepresentationDistinction;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections
{
    public class DictionaryCompareStrategy : BaseCollectionsCompareStrategy
    {
        public Distinctions CompareDictionary<TKey, TValue>(IDictionary<TKey, TValue> valueA,
            IDictionary<TKey, TValue> valueB, string propertyName)
        {
            var diff = new Distinctions();
            if (valueA.Count != valueB.Count)
                return Distinctions.Create("Dictionary has different length", valueA.Count, valueB.Count);
            foreach (var kvp in valueA)
            {
                if (!valueB.TryGetValue(kvp.Key, out var secondValue))
                    diff.Add(new Distinction(kvp.Key.ToString(), "Should be", "Does not exist"));

                var diffRes = Comparator.Compare(kvp.Value, secondValue, $"{propertyName}[{kvp.Key}]");
                diff.AddRange(diffRes);
            }

            return diff;
        }

        public override Distinctions Compare<T>(T valueA, T valueB, string propertyName)
        {
            var genericArguments = valueA.GetType().GetGenericArguments();
            return (Distinctions) GetType().GetMethod("CompareDictionary").MakeGenericMethod(genericArguments)
                .Invoke(this, new[] {(object) valueA, valueB, propertyName});
        }

        public override bool IsValid(Type member)
        {
            return member.IsGenericType && member.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }
    }
}