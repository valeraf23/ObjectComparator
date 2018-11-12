using System;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Strategies.Implementations
{
    public sealed class CompareValueTypesStrategy : ICompareStructStrategy
    {
        public bool IsValid(Type member) => member.IsValueType || member == typeof(string);

        public Distinctions Compare<T>(T valueA, T valueB, string propertyName) =>
            Compare(propertyName, valueA, valueB);

        public Distinctions Compare<T>(string propertyName, T valueA, T valueB) => Distinctions
            .CreateFor<T>(propertyName, valueA, valueB).WhenNot((a, b) => a.Equals(b));
    }
}