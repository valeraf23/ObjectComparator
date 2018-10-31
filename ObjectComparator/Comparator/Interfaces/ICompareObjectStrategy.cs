using System.Collections.Generic;

namespace ObjectComparator.Comparator.Interfaces
{
    public interface ICompareObjectStrategy : IStrategy
    {
        IList<string> Ignore { get; set; }
        IDictionary<string, ICompareValues> Strategies { get; set; }
        DistinctionsCollection Compare<T>(T valueA, T valueB);
    }
}
