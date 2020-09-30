using System;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;

namespace ObjectsComparator.Comparator.Strategies.Implementations
{
    public sealed class ComparePrimitiveTypesStrategy : ICompareStructStrategy
    {
        public bool IsValid(Type member) => member.IsPrimitiveOrString();

        public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull =>
            DeepEqualityResult
                .CreateFor(propertyName, expected, actual).WhenNot((a, b) => a.Equals(b));
    }
}
