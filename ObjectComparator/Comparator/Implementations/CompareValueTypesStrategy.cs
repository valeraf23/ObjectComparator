using System;
using ObjectComparator.Comparator.Interfaces;

namespace ObjectComparator.Comparator.Implementations
{
    public sealed class CompareValueTypesStrategy : ICompareStructStrategy
    {
        public bool IsValid(Type member) => member.IsValueType || member == typeof(string);

        public DistinctionsCollection Compare<T>(T valueA, T valueB, string propertyName) =>
            Compare(propertyName, valueA, valueB);

        public DistinctionsCollection Compare<T>(string propertyName, T valueA, T valueB)
        {
            var distinctionsCollection = new DistinctionsCollection();
            return valueA.Equals(valueB)
                ? distinctionsCollection
                : distinctionsCollection.Add(new Distinction(propertyName, valueA, valueB));
        }
    }
}