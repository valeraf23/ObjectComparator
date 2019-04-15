using System;
using System.Collections.Generic;

namespace ObjectsComparator.Comparator.Strategies.Interfaces
{
    public interface ICompareObjectStrategy : IStrategy
    {
        Func<string, bool> Ignore { get; set; }
        IDictionary<string, ICompareValues> Strategies { get; set; }
    }
}