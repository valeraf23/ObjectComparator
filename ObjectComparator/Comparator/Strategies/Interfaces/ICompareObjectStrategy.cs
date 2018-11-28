using System.Collections.Generic;

namespace ObjectsComparator.Comparator.Strategies.Interfaces
{
    public interface ICompareObjectStrategy : IStrategy
    {
        IList<string> Ignore { get; set; }
        IDictionary<string, ICompareValues> Strategies { get; set; }
    }
}