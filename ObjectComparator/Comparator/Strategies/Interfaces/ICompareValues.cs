using ObjectsComparator.Comparator.RepresentationDistinction;

namespace ObjectsComparator.Comparator.Strategies.Interfaces;

public interface ICompareValues
{
    DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull;
}