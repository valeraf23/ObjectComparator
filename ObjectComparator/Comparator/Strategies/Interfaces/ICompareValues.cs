using ObjectsComparator.Comparator.RepresentationDistinction;

namespace ObjectsComparator.Comparator.Strategies.Interfaces
{
    public interface ICompareValues
    {
        DistinctionsCollection Compare<T>(T valueA, T valueB, string propertyName);
    }
}