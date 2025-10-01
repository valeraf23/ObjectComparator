using ObjectsComparator.Comparator.Strategies;
using System.Collections.Generic;

namespace ObjectsComparator.Comparator.Helpers
{
    public class ComparatorOptions
    {
        public HashSet<StrategyType> StrategyTypeSkipList { get; set; } = new HashSet<StrategyType>();
    }
}
