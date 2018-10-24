using System.Collections.Generic;
using ObjectComparator.Comparator.StrategiesForCertainProperties;

namespace ObjectComparator.Comparator.Interfaces
{
    public interface ICompareObjectStrategy : IStrategy
    {
        IList<string> Ignore { get; set; }
        IList<IMemberStrategy> Strategies { get; set; }
    }
}
