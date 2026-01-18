using ObjectsComparator.Comparator.Strategies;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectsComparator.Comparator.Helpers
{
    /// <summary>
    /// Unified fluent configuration for deep equality comparisons.
    /// Combines options, strategies, type strategies, and ignore rules into a single API.
    /// </summary>
    /// <typeparam name="T">The type of the expected object (used for property-based strategy definitions).</typeparam>
    /// <example>
    /// <code>
    /// expected.DeeplyEquals(actual, config => config
    ///     .Ignore("Id", "CreatedDate")
    ///     .AllowDifferentTypes()
    ///     .WithTypeStrategies(ts => ts.Set&lt;string&gt;((e, a) => 
    ///         string.Equals(e, a, StringComparison.OrdinalIgnoreCase)))
    ///     .WithStrategies(s => s.Set(x => x.Name, (e, a) => e == a)));
    /// </code>
    /// </example>
    public sealed class ComparisonConfig<T>
    {
        private readonly HashSet<StrategyType> _skippedStrategies = new();
        private readonly List<string> _ignoreProperties = new();
        private Dictionary<string, ICustomCompareValues> _propertyStrategies = new();
        private Dictionary<Type, ICustomCompareValues> _typeStrategies = new();
        private Func<string, bool>? _customIgnoreStrategy;

        /// <summary>
        /// Gets whether different types are allowed in comparison.
        /// </summary>
        public bool DifferentTypesAllowed { get; private set; }

        /// <summary>
        /// Allows comparing objects of different types by matching properties by name.
        /// </summary>
        /// <param name="allow">Whether to allow different types (default: true).</param>
        /// <returns>The config instance for method chaining.</returns>
        public ComparisonConfig<T> AllowDifferentTypes(bool allow = true)
        {
            DifferentTypesAllowed = allow;
            return this;
        }

        /// <summary>
        /// Skips the specified comparison strategies.
        /// </summary>
        /// <param name="strategies">Strategies to skip (e.g., StrategyType.OverridesEquals).</param>
        /// <returns>The config instance for method chaining.</returns>
        public ComparisonConfig<T> Skip(params StrategyType[] strategies)
        {
            if (strategies != null)
            {
                foreach (var s in strategies)
                {
                    _skippedStrategies.Add(s);
                }
            }
            return this;
        }

        /// <summary>
        /// Specifies property names to exclude from comparison.
        /// </summary>
        /// <param name="propertyNames">Property names to ignore (e.g., "PropertyName" or "Nested.Property").</param>
        /// <returns>The config instance for method chaining.</returns>
        public ComparisonConfig<T> Ignore(params string[] propertyNames)
        {
            if (propertyNames != null)
            {
                _ignoreProperties.AddRange(propertyNames);
            }
            return this;
        }

        /// <summary>
        /// Sets a custom ignore strategy function.
        /// When set, this takes precedence over properties specified via <see cref="Ignore"/>.
        /// </summary>
        /// <param name="ignoreStrategy">A function that returns true for property paths that should be ignored.</param>
        /// <returns>The config instance for method chaining.</returns>
        public ComparisonConfig<T> IgnoreWhen(Func<string, bool> ignoreStrategy)
        {
            _customIgnoreStrategy = ignoreStrategy;
            return this;
        }

        /// <summary>
        /// Configures type-based comparison strategies that apply to all properties of a specific type.
        /// </summary>
        /// <param name="builder">A function to configure type strategies.</param>
        /// <returns>The config instance for method chaining.</returns>
        /// <example>
        /// <code>
        /// config.WithTypeStrategies(ts => ts
        ///     .Set&lt;string&gt;((e, a) => string.Equals(e, a, StringComparison.OrdinalIgnoreCase))
        ///     .Set&lt;DateTime&gt;((e, a) => e.Date == a.Date));
        /// </code>
        /// </example>
        public ComparisonConfig<T> WithTypeStrategies(Func<TypeStrategies, TypeStrategies> builder)
        {
            if (builder != null)
            {
                var strategies = builder(new TypeStrategies());
                _typeStrategies = strategies.ToDictionary();
            }
            return this;
        }

        /// <summary>
        /// Configures property-specific comparison strategies.
        /// </summary>
        /// <param name="builder">A function to configure property strategies.</param>
        /// <returns>The config instance for method chaining.</returns>
        /// <example>
        /// <code>
        /// config.WithStrategies(s => s
        ///     .Set(x => x.Name, (e, a) => string.Equals(e, a, StringComparison.OrdinalIgnoreCase))
        ///     .Set(x => x.Price, (e, a) => Math.Abs(e - a) &lt; 0.01m));
        /// </code>
        /// </example>
        public ComparisonConfig<T> WithStrategies(Func<Strategies<T>, Strategies<T>> builder)
        {
            if (builder != null)
            {
                var strategies = builder(new Strategies<T>());
                _propertyStrategies = strategies.ToDictionary(x => x.Key, x => x.Value);
            }
            return this;
        }

        /// <summary>
        /// Configures property-specific comparison strategies using pre-built strategies.
        /// </summary>
        /// <param name="strategies">Pre-configured property strategies.</param>
        /// <returns>The config instance for method chaining.</returns>
        public ComparisonConfig<T> WithStrategies(Strategies<T> strategies)
        {
            if (strategies != null)
            {
                _propertyStrategies = strategies.ToDictionary(x => x.Key, x => x.Value);
            }
            return this;
        }

        #region Internal Build Methods

        internal ComparatorOptions BuildOptions()
        {
            var options = new ComparatorOptions(_skippedStrategies.ToArray());
            
            if (DifferentTypesAllowed)
            {
                options.AllowDifferentTypes();
            }

            if (_typeStrategies.Count > 0)
            {
                options.WithTypeStrategies(_ => 
                {
                    var ts = new TypeStrategies();
                    // Copy type strategies - this is a workaround since TypeStrategies doesn't expose internal dict directly
                    return ts;
                });
                // Directly set the type strategies on options
                foreach (var kvp in _typeStrategies)
                {
                    options.TypeStrategies[kvp.Key] = kvp.Value;
                }
            }

            return options;
        }

        internal Dictionary<string, ICustomCompareValues> GetPropertyStrategies() => _propertyStrategies;

        internal Dictionary<Type, ICustomCompareValues> GetTypeStrategies() => _typeStrategies;

        internal IEnumerable<string> GetIgnoreProperties() => _ignoreProperties;

        internal Func<string, bool>? GetCustomIgnoreStrategy() => _customIgnoreStrategy;

        internal ISet<StrategyType> GetSkippedStrategies() => _skippedStrategies;

        #endregion
    }
}
