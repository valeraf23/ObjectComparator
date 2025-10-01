using ObjectsComparator.Comparator.Strategies;
using System.Collections.Generic;

namespace ObjectsComparator.Comparator.Helpers
{
    public class ComparatorOptions
    {
        public HashSet<StrategyType> StrategyTypeSkipList { get; set; } = new HashSet<StrategyType>();

        public ComparatorOptions(params StrategyType[] skipList)
        {
            StrategyTypeSkipList = new HashSet<StrategyType>(skipList);
        }

        public ComparatorOptions() {}
        public static ComparatorOptions Create(params StrategyType[] skipList)
        {
            return new ComparatorOptions(skipList);
        }
    }
}
