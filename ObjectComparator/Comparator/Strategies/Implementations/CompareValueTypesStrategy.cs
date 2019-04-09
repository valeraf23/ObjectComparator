using System;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Strategies.Implementations
{
    public sealed class CompareValueTypesStrategy : ICompareStructStrategy
    {
        public bool IsValid(Type member) => member.IsValueType || member == typeof(string);

        public Distinctions Compare<T>(T expected, T actual, string propertyName) =>
            Compare(propertyName, expected, actual);

        public Distinctions Compare<T>(string propertyName, T expected, T actual) =>
            Distinctions
                .CreateFor<T>(propertyName, expected, actual).WhenNot((a, b) => a.Equals(b));
    }
}