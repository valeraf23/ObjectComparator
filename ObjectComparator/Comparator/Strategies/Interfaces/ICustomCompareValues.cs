using ObjectsComparator.Comparator.RepresentationDistinction;

namespace ObjectsComparator.Comparator.Strategies.Interfaces
{
    public interface ICustomCompareValues
    {
        DeepEqualityResult Compare<T>(T expected, T actual, string propertyName);
    }
}