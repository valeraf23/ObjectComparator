using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using System;

namespace ObjectsComparator.Comparator;

public interface IComparator
{
    ComparatorOptions Options { get; }

    DeepEqualityResult Compare<T>(T expected, T actual);

    DeepEqualityResult CompareWithTypes(object? expected, object? actual, string propertyName,
        Type expectedType, Type actualType);
}