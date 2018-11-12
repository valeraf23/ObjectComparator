using ObjectsComparator.Comparator.RepresentationDistinction;

namespace ObjectsComparator.Comparator.Strategies.Interfaces
{
    public interface ICompareValues
    {
        Distinctions Compare<T>(T valueA, T valueB, string propertyName);
    }
}