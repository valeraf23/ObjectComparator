namespace ObjectComparator.Comparator.Interfaces
{
    public interface ICompareStructStrategy : IStrategy
    {
        DistinctionsCollection Compare<T>(string propertyName,T valueA, T valueB);
    }
}