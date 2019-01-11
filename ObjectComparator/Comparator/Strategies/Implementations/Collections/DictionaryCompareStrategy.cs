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
            foreach (var (key, value) in valueA)
            {
                if (!valueB.TryGetValue(key, out var secondValue))
                    diff.Add(new Distinction(key.ToString(), "Should be", "Does not exist"));

                var diffRes = Comparator.Compare(value, secondValue, $"{propertyName}[{key}]");
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

        public override bool IsValid(Type member) =>
            member.IsGenericType && member.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }
}