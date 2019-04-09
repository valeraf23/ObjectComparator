using ObjectsComparator.Comparator.RepresentationDistinction;

namespace ObjectsComparator.Comparator.Strategies.Interfaces
{
    public interface ICompareValues
    {
        Distinctions Compare<T>(T expected, T actual, string propertyName);
    }
}