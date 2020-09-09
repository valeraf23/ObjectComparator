using System;
using System.Collections.Generic;

namespace ObjectsComparator.Comparator.Strategies.Interfaces
{
    public interface ICompareObjectStrategy : IStrategy
    {
        Func<string, bool> Ignore { get; set; }
        Dictionary<string, ICompareValues> Strategies { get; set; }
    }
}