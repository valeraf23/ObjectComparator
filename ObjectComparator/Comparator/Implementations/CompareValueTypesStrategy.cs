using System;
using ObjectsComparator.Comparator.Interfaces;

namespace ObjectsComparator.Comparator.Implementations
{
    public sealed class CompareValueTypesStrategy : ICompareStructStrategy
    {
        public bool IsValid(Type member) => member.IsValueType || member == typeof(string);

        public DistinctionsCollection Compare<T>(T valueA, T valueB, string propertyName) =>
            Compare(propertyName, valueA, valueB);

        public DistinctionsCollection Compare<T>(string propertyName, T valueA, T valueB) => DistinctionsCollection
            .CreateFor<T>(propertyName, valueA, valueB).WhenNot((a, b) => a.Equals(b));
    }
}