using System;
using System.Collections.Generic;
using ObjectsComparator.Comparator.Strategies;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties;

namespace ObjectsComparator.Comparator.Helpers
{
    public sealed class ComparatorOptions
    {
        public ISet<StrategyType> SkippedStrategies { get; }
        public bool DifferentTypesAllowed { get; private set; }
        
        internal Dictionary<Type, ICustomCompareValues> TypeStrategies { get; private set; } = new();

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

        public ComparatorOptions AllowDifferentTypes(bool allow = true)
        {
            DifferentTypesAllowed = allow;
            return this;
        }

        /// <summary>
        /// Configures type-based comparison strategies that apply to all properties of a specific type.
        /// </summary>
        /// <param name="builder">A function to configure type strategies.</param>
        /// <returns>The options instance for method chaining.</returns>
        /// <example>
        /// <code>
        /// options.WithTypeStrategies(s => s
        ///     .Set(typeof(string), (e, a) => string.Equals((string?)e, (string?)a, StringComparison.OrdinalIgnoreCase))
        ///     .Set&lt;DateTime&gt;((e, a) => e.Date == a.Date));
        /// </code>
        /// </example>
        public ComparatorOptions WithTypeStrategies(Func<TypeStrategies, TypeStrategies> builder)
        {
            if (builder is null)
            {
                return this;
            }

            var strategies = builder(new TypeStrategies());
            TypeStrategies = strategies.ToDictionary();
            return this;
        }

        internal bool IsSkipped(StrategyType strategy)
        {
            return SkippedStrategies.Contains(strategy);
        }
    }
}
