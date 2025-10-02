using System;
using System.Collections.Generic;
using ObjectsComparator.Comparator.Strategies;

namespace ObjectsComparator.Comparator.Helpers
{
    public sealed class ComparatorOptions
    {
        public ISet<StrategyType> SkippedStrategies { get; }

        public ComparatorOptions(params StrategyType[] skipList)
        {
            SkippedStrategies = new HashSet<StrategyType>(skipList ?? Array.Empty<StrategyType>());
        }

        public ComparatorOptions() : this(Array.Empty<StrategyType>())
        {
        }

        public static ComparatorOptions SkipStrategies(params StrategyType[] skipList)
        {
            return new ComparatorOptions(skipList);
        }

        public ComparatorOptions Skip(params StrategyType[] strategies)
        {
            if (strategies is null)
            {
                return this;
            }

            foreach (var s in strategies)
            {
                SkippedStrategies.Add(s);
            }

            return this;
        }

        internal bool IsSkipped(StrategyType strategy)
        {
            return SkippedStrategies.Contains(strategy);
        }
    }
}