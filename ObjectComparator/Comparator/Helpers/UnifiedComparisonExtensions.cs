using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties;
using System;
using System.Linq;

namespace ObjectsComparator.Comparator.Helpers
{
    /// <summary>
    /// Extension methods for unified fluent configuration of deep comparisons.
    /// </summary>
    internal static class UnifiedComparisonExtensions
    {
        /// <summary>
        /// Performs a deep equality comparison with unified fluent configuration.
        /// This is the recommended way to configure comparisons with multiple options.
        /// </summary>
        /// <typeparam name="T">The type of the expected object (used for strategy definitions).</typeparam>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object to compare against expected.</param>
        /// <param name="configure">A function to configure the comparison using fluent API.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        /// <example>
        /// <code>
        /// // Same types
        /// var result = expected.DeepCompare(actual, config => config
        ///     .Ignore("Id", "CreatedDate")
        ///     .WithTypeStrategies(ts => ts.Set&lt;string&gt;((e, a) => 
        ///         string.Equals(e, a, StringComparison.OrdinalIgnoreCase)))
        ///     .WithStrategies(s => s.Set(x => x.Name, (e, a) => e == a)));
        /// 
        /// // Different types (use AllowDifferentTypes)
        /// var result = expectedDto.DeepCompare&lt;VehicleDto&gt;(actualEntity, config => config
        ///     .AllowDifferentTypes()
        ///     .Ignore("InternalCode"));
        /// </code>
        /// </example>
        internal static DeepEqualityResult DeepCompare<T>(this T expected, object? actual,
            Action<ComparisonConfig<T>> configure)
        {
            var config = new ComparisonConfig<T>();
            configure?.Invoke(config);
            return ExecuteComparison(expected, actual, config);
        }

        private static DeepEqualityResult ExecuteComparison<T>(object? expected, object? actual, ComparisonConfig<T> config)
        {
            var typeName = ComparisonHelper.GetIgnoreTypeName(expected, actual, typeof(T),
                new ComparatorOptions().AllowDifferentTypes(config.DifferentTypesAllowed));

            var ignoreStrategy = config.GetCustomIgnoreStrategy()
                ?? ComparisonHelper.CreateIgnoreStrategy(config.GetIgnoreProperties(), typeName);

            var options = new ComparatorOptions(config.GetSkippedStrategies().ToArray());
            if (config.DifferentTypesAllowed)
            {
                options.AllowDifferentTypes();
            }

            // Copy type strategies to options
            foreach (var kvp in config.GetTypeStrategies())
            {
                options.TypeStrategies[kvp.Key] = kvp.Value;
            }

            var comparator = new Comparator(config.GetPropertyStrategies(), ignoreStrategy, options);
            return comparator.Compare(expected, actual);
        }
    }
}
