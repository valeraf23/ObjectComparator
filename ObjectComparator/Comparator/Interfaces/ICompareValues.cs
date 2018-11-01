namespace ObjectsComparator.Comparator.Interfaces
{
    public interface ICompareValues
    {
        DistinctionsCollection Compare<T>(T valueA, T valueB, string propertyName);
    }
}