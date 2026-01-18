using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectsComparator.Comparator.Helpers
{
    /// <summary>
    /// Fluent builder for configuring and executing deep equality comparisons.
    /// </summary>
    /// <typeparam name="T">The type of the expected object (used for strategy definitions).</typeparam>
    /// <example>
    /// <code>
    /// var result = expected.DeepCompare(actual)
    ///     .IgnoreProperties("Id", "CreatedDate")
    ///     .WithOptions(o => o.AllowDifferentTypes())
    ///     .WithStrategies(s => s.Set(x => x.Name, (e, a) => string.Equals(e, a, StringComparison.OrdinalIgnoreCase)))
    ///     .Compare();
    /// </code>
    /// </example>
    public sealed class DeepComparisonBuilder<T>
    {
        private readonly object? _expected;
        private readonly object? _actual;
        private readonly Type _defaultType;
        private readonly ComparatorOptions _options = new ComparatorOptions();
        private readonly List<string> _ignoreProperties = new List<string>();
        private Dictionary<string, ICustomCompareValues>? _customStrategies;
        private Func<string, bool>? _customIgnoreStrategy;

        internal DeepComparisonBuilder(T expected, T actual)
        {
            _expected = expected;
            _actual = actual;
            _defaultType = typeof(T);
        }

        internal DeepComparisonBuilder(object? expected, object? actual, Type defaultType)
        {
            _expected = expected;
            _actual = actual;
            _defaultType = defaultType;
        }

        /// <summary>
        /// Specifies property names to exclude from comparison.
        /// </summary>
        /// <param name="propertyNames">Property names to ignore (e.g., "PropertyName" or "Nested.Property").</param>
        /// <returns>The builder for method chaining.</returns>
        public DeepComparisonBuilder<T> IgnoreProperties(params string[] propertyNames)
        {
            if (propertyNames != null)
            {
                _ignoreProperties.AddRange(propertyNames);
            }
            return this;
        }

        /// <summary>
        /// Configures comparison options using a builder action.
        /// </summary>
        /// <param name="optionsBuilder">An action to configure comparison options.</param>
        /// <returns>The builder for method chaining.</returns>
        public DeepComparisonBuilder<T> WithOptions(Action<ComparatorOptions> optionsBuilder)
        {
            optionsBuilder?.Invoke(_options);
            return this;
        }

        /// <summary>
        /// Configures comparison options using pre-built options.
        /// </summary>
        /// <param name="options">Pre-configured comparison options.</param>
        /// <returns>The builder for method chaining.</returns>
        public DeepComparisonBuilder<T> WithOptions(ComparatorOptions options)
        {
            if (options != null)
            {
                foreach (var strategy in options.SkippedStrategies)
                {
                    _options.Skip(strategy);
                }
                if (options.DifferentTypesAllowed)
                {
                    _options.AllowDifferentTypes();
                }
            }
            return this;
        }

        /// <summary>
        /// Configures custom comparison strategies for specific properties.
        /// </summary>
        /// <param name="strategies">A function to configure custom comparison strategies.</param>
        /// <returns>The builder for method chaining.</returns>
        public DeepComparisonBuilder<T> WithStrategies(Func<Strategies<T>, Strategies<T>> strategies)
        {
            if (strategies != null)
            {
                var customStrategies = strategies(new Strategies<T>());
                _customStrategies = customStrategies.ToDictionary(x => x.Key, x => x.Value);
            }
            return this;
        }

        /// <summary>
        /// Configures custom comparison strategies using pre-built strategies.
        /// </summary>
        /// <param name="strategies">Pre-configured custom comparison strategies.</param>
        /// <returns>The builder for method chaining.</returns>
        public DeepComparisonBuilder<T> WithStrategies(Strategies<T> strategies)
        {
            if (strategies != null)
            {
                _customStrategies = strategies.ToDictionary(x => x.Key, x => x.Value);
            }
            return this;
        }

        /// <summary>
        /// Configures a custom ignore strategy function.
        /// When set, this takes precedence over properties specified via <see cref="IgnoreProperties"/>.
        /// </summary>
        /// <param name="ignoreStrategy">A function that returns true for property paths that should be ignored.</param>
        /// <returns>The builder for method chaining.</returns>
        public DeepComparisonBuilder<T> WithIgnoreStrategy(Func<string, bool> ignoreStrategy)
        {
            _customIgnoreStrategy = ignoreStrategy;
            return this;
        }

        /// <summary>
        /// Executes the deep equality comparison with the configured settings.
        /// </summary>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        public DeepEqualityResult Compare()
        {
            var ignoreStrategy = _customIgnoreStrategy ?? BuildIgnoreStrategy();
            var customStrategies = _customStrategies ?? new Dictionary<string, ICustomCompareValues>();

            var comparator = new Comparator(customStrategies, ignoreStrategy, _options);
            return comparator.Compare(_expected, _actual);
        }

        private Func<string, bool> BuildIgnoreStrategy()
        {
            var typeName = ComparisonHelper.GetIgnoreTypeName(_expected, _actual, _defaultType, _options);
            return ComparisonHelper.CreateIgnoreStrategy(_ignoreProperties, typeName);
        }
    }
}
