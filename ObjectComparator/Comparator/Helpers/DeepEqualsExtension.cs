using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties;
using ObjectsComparator.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ObjectsComparator.Comparator.Helpers
{
    /// <summary>
    /// Provides extension methods for performing deep equality comparisons between objects.
    /// </summary>
    public static class DeepEqualsExtension
    {
        /// <summary>
        /// Converts the comparison result to a JSON string representation.
        /// </summary>
        /// <param name="distinctions">The comparison result to convert.</param>
        /// <returns>A JSON string representing the comparison distinctions.</returns>
        public static string ToJson(this DeepEqualityResult distinctions)
        {
            return DeepComparisonJsonConverter.ToJson(distinctions);
        }

        #region Basic Comparisons

        /// <summary>
        /// Performs a deep equality comparison between two objects of the same type.
        /// </summary>
        /// <typeparam name="T">The type of objects to compare.</typeparam>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object to compare against expected.</param>
        /// <param name="ignore">Property names to exclude from comparison (e.g., "PropertyName" or "Nested.Property").</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        /// <example>
        /// <code>
        /// var result = expected.DeeplyEquals(actual, "Id", "CreatedDate");
        /// </code>
        /// </example>
        public static DeepEqualityResult DeeplyEquals<T>(this T expected, T actual, params string[] ignore)
        {
            var ignoreStrategy = ComparisonHelper.CreateIgnoreStrategy(ignore, typeof(T).ToFriendlyTypeName());
            return DeeplyEquals(expected, actual, new Dictionary<string, ICustomCompareValues>(),
                ignoreStrategy, new ComparatorOptions());
        }

        #endregion

        #region Comparisons with Options (Same and Different Types)

        /// <summary>
        /// Performs a deep equality comparison with unified fluent configuration.
        /// This is the recommended way to configure comparisons with multiple options.
        /// </summary>
        /// <typeparam name="T">The type of the expected object (used for strategy definitions).</typeparam>
        /// <typeparam name="TActual">The type of the actual object.</typeparam>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object to compare against expected.</param>
        /// <param name="configure">A function to configure the comparison using fluent API.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        /// <example>
        /// <code>
        /// var result = expected.DeeplyEquals(actual, config => config
        ///     .Ignore("Id", "CreatedDate")
        ///     .AllowDifferentTypes()
        ///     .WithTypeStrategies(ts => ts.Set&lt;string&gt;((e, a) => 
        ///         string.Equals(e, a, StringComparison.OrdinalIgnoreCase)))
        ///     .WithStrategies(s => s.Set(x => x.Name, (e, a) => e == a)));
        /// </code>
        /// </example>
        public static DeepEqualityResult DeeplyEquals<T, TActual>(this T expected, TActual actual,
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

            foreach (var kvp in config.GetTypeStrategies())
            {
                options.TypeStrategies[kvp.Key] = kvp.Value;
            }

            var comparator = new Comparator(config.GetPropertyStrategies(), ignoreStrategy, options);
            return comparator.Compare(expected, actual);
        }

        /// <summary>
        /// Performs a deep equality comparison with pre-configured options.
        /// </summary>
        /// <typeparam name="T">The type of objects to compare.</typeparam>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object to compare against expected.</param>
        /// <param name="options">The comparison options.</param>
        /// <param name="ignore">Property names to exclude from comparison.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        public static DeepEqualityResult DeeplyEquals<T>(this T expected, T actual, ComparatorOptions options,
            params string[] ignore)
        {
            var ignoreStrategy = ComparisonHelper.CreateIgnoreStrategy(ignore, typeof(T).ToFriendlyTypeName());
            return DeeplyEquals(expected, actual, new Dictionary<string, ICustomCompareValues>(),
                ignoreStrategy, options);
        }

        #endregion

        #region Comparisons with Custom Strategies

        /// <summary>
        /// Performs a deep equality comparison with custom comparison strategies for specific properties.
        /// </summary>
        /// <typeparam name="T">The type of objects to compare.</typeparam>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object to compare against expected.</param>
        /// <param name="strategies">A function to configure custom comparison strategies using a fluent builder.</param>
        /// <param name="ignore">Property names to exclude from comparison.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        /// <example>
        /// <code>
        /// // Custom string comparison: treat null and empty as equal
        /// var result = expected.DeeplyEquals(actual,
        ///     strategy => strategy
        ///         .Set(x => x.Name, (exp, act) => 
        ///             (string.IsNullOrEmpty(exp) &amp;&amp; string.IsNullOrEmpty(act)) || exp == act),
        ///     "Id");
        /// </code>
        /// </example>
        public static DeepEqualityResult DeeplyEquals<T>(this T expected, T actual,
            Func<Strategies<T>, Strategies<T>> strategies,
            params string[] ignore)
        {
            var ignoreStrategy = ComparisonHelper.CreateIgnoreStrategy(ignore, typeof(T).ToFriendlyTypeName());
            var customStrategies = strategies(new Strategies<T>());
            return DeeplyEquals(expected, actual, customStrategies.ToDictionary(x => x.Key, x => x.Value),
                ignoreStrategy);
        }

        /// <summary>
        /// Performs a deep equality comparison with pre-built custom strategies.
        /// </summary>
        /// <typeparam name="T">The type of objects to compare.</typeparam>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object to compare against expected.</param>
        /// <param name="strategies">Pre-configured custom comparison strategies.</param>
        /// <param name="ignore">Property names to exclude from comparison.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        public static DeepEqualityResult DeeplyEquals<T>(this T expected, T actual, Strategies<T> strategies,
            params string[] ignore)
        {
            var ignoreStrategy = ComparisonHelper.CreateIgnoreStrategy(ignore, typeof(T).ToFriendlyTypeName());
            return DeeplyEquals(expected, actual, strategies.ToDictionary(x => x.Key, x => x.Value),
                ignoreStrategy);
        }

        #endregion

        #region Collection Comparisons with Custom Strategies

        /// <summary>
        /// Performs a deep equality comparison between two collections with custom comparison strategies.
        /// </summary>
        /// <typeparam name="T">The element type of the collections.</typeparam>
        /// <param name="expected">The expected collection.</param>
        /// <param name="actual">The actual collection to compare against expected.</param>
        /// <param name="strategies">A function to configure custom comparison strategies for the element type.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        public static DeepEqualityResult DeeplyEquals<T>(this IEnumerable<T> expected, IEnumerable<T> actual,
            Func<Strategies<T>, Strategies<T>> strategies)
        {
            var ignoreStrategy = ComparisonHelper.CreateIgnoreStrategy(Array.Empty<string>(), typeof(T).ToFriendlyTypeName());
            var customStrategies = strategies(new Strategies<T>());
            return DeeplyEquals(expected, actual, customStrategies.ToDictionary(x => x.Key, x => x.Value),
                ignoreStrategy);
        }

        /// <summary>
        /// Performs a deep equality comparison between two collections with custom comparison strategies and properties to ignore.
        /// </summary>
        /// <typeparam name="T">The element type of the collections.</typeparam>
        /// <param name="expected">The expected collection.</param>
        /// <param name="actual">The actual collection to compare against expected.</param>
        /// <param name="strategies">A function to configure custom comparison strategies.</param>
        /// <param name="ignore">Property names to exclude from comparison.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        public static DeepEqualityResult DeeplyEquals<T>(this IEnumerable<T> expected, IEnumerable<T> actual,
            Func<Strategies<T>, Strategies<T>> strategies,
            params string[] ignore)
        {
            var ignoreStrategy = ComparisonHelper.CreateIgnoreStrategy(ignore, typeof(T).ToFriendlyTypeName());
            var customStrategies = strategies(new Strategies<T>());
            return DeeplyEquals(expected, actual, customStrategies.ToDictionary(x => x.Key, x => x.Value),
                ignoreStrategy);
        }

        /// <summary>
        /// Performs a deep equality comparison between two collections of different types with custom comparison strategies.
        /// </summary>
        /// <typeparam name="T">The element type of the expected collection (used for strategy definitions).</typeparam>
        /// <typeparam name="TActual">The element type of the actual collection.</typeparam>
        /// <param name="expected">The expected collection.</param>
        /// <param name="actual">The actual collection to compare against expected.</param>
        /// <param name="strategies">A function to configure custom comparison strategies based on the expected element type.</param>
        /// <param name="optionsBuilder">An action to configure comparison options (typically to call AllowDifferentTypes()).</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        public static DeepEqualityResult DeeplyEquals<T, TActual>(this IEnumerable<T> expected, IEnumerable<TActual> actual,
            Func<Strategies<T>, Strategies<T>> strategies,
            Action<ComparatorOptions> optionsBuilder)
        {
            var options = ComparisonHelper.BuildOptions(optionsBuilder);
            var ignoreStrategy = ComparisonHelper.CreateIgnoreStrategy(Array.Empty<string>(), typeof(T).ToFriendlyTypeName());
            var customStrategies = strategies(new Strategies<T>());
            return DeeplyEquals<object?>(expected, actual, customStrategies.ToDictionary(x => x.Key, x => x.Value),
                ignoreStrategy, options);
        }

        /// <summary>
        /// Performs a deep equality comparison between two collections of different types with custom comparison strategies and properties to ignore.
        /// </summary>
        /// <typeparam name="T">The element type of the expected collection (used for strategy definitions).</typeparam>
        /// <typeparam name="TActual">The element type of the actual collection.</typeparam>
        /// <param name="expected">The expected collection.</param>
        /// <param name="actual">The actual collection to compare against expected.</param>
        /// <param name="strategies">A function to configure custom comparison strategies based on the expected element type.</param>
        /// <param name="optionsBuilder">An action to configure comparison options (typically to call AllowDifferentTypes()).</param>
        /// <param name="ignore">Property names to exclude from comparison.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        public static DeepEqualityResult DeeplyEquals<T, TActual>(this IEnumerable<T> expected, IEnumerable<TActual> actual,
            Func<Strategies<T>, Strategies<T>> strategies,
            Action<ComparatorOptions> optionsBuilder,
            params string[] ignore)
        {
            var options = ComparisonHelper.BuildOptions(optionsBuilder);
            var ignoreStrategy = ComparisonHelper.CreateIgnoreStrategy(ignore, typeof(T).ToFriendlyTypeName());
            var customStrategies = strategies(new Strategies<T>());
            return DeeplyEquals<object?>(expected, actual, customStrategies.ToDictionary(x => x.Key, x => x.Value),
                ignoreStrategy, options);
        }

        #endregion

        #region Comparisons with Options and Strategies (Full Configuration)

        /// <summary>
        /// Performs a deep equality comparison with both custom strategies and configurable options.
        /// This is the most flexible overload, allowing you to combine all comparison features.
        /// </summary>
        /// <typeparam name="T">The type of objects to compare.</typeparam>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object to compare against expected.</param>
        /// <param name="strategies">A function to configure custom comparison strategies.</param>
        /// <param name="optionsBuilder">An action to configure comparison options.</param>
        /// <param name="ignore">Property names to exclude from comparison.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        /// <example>
        /// <code>
        /// // Combine: different types allowed + custom string strategy + skip properties
        /// var result = expected.DeeplyEquals(actual,
        ///     strategy => strategy
        ///         .Set(x => x.Model, (exp, act) => 
        ///             (string.IsNullOrEmpty(exp) &amp;&amp; string.IsNullOrEmpty(act)) || exp == act)
        ///         .Set(x => x.Description, (exp, act) => 
        ///             (string.IsNullOrEmpty(exp) &amp;&amp; string.IsNullOrEmpty(act)) || exp == act),
        ///     options => options.AllowDifferentTypes(),
        ///     "Id", "CreatedDate");
        /// </code>
        /// </example>
        public static DeepEqualityResult DeeplyEquals<T>(this T expected, T actual,
            Func<Strategies<T>, Strategies<T>> strategies,
            Action<ComparatorOptions> optionsBuilder,
            params string[] ignore)
        {
            var options = ComparisonHelper.BuildOptions(optionsBuilder);
            var ignoreStrategy = ComparisonHelper.CreateIgnoreStrategy(ignore, 
                ComparisonHelper.GetIgnoreTypeName(expected, actual, typeof(T), options));
            var customStrategies = strategies(new Strategies<T>());
            return DeeplyEquals(expected, actual, customStrategies.ToDictionary(x => x.Key, x => x.Value),
                ignoreStrategy, options);
        }

        /// <summary>
        /// Performs a deep equality comparison with pre-built strategies and options.
        /// </summary>
        /// <typeparam name="T">The type of objects to compare.</typeparam>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object to compare against expected.</param>
        /// <param name="strategies">Pre-configured custom comparison strategies.</param>
        /// <param name="options">The comparison options.</param>
        /// <param name="ignore">Property names to exclude from comparison.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        public static DeepEqualityResult DeeplyEquals<T>(this T expected, T actual, Strategies<T> strategies,
            ComparatorOptions options, params string[] ignore)
        {
            var ignoreStrategy = ComparisonHelper.CreateIgnoreStrategy(ignore, 
                ComparisonHelper.GetIgnoreTypeName(expected, actual, typeof(T), options));
            return DeeplyEquals(expected, actual, strategies.ToDictionary(x => x.Key, x => x.Value),
                ignoreStrategy, options);
        }

        /// <summary>
        /// Performs a deep equality comparison with strategies builder, pre-built options, and properties to ignore.
        /// </summary>
        /// <typeparam name="T">The type of objects to compare.</typeparam>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object to compare against expected.</param>
        /// <param name="strategies">A function to configure custom comparison strategies.</param>
        /// <param name="options">Pre-configured comparison options.</param>
        /// <param name="ignore">Property names to exclude from comparison.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        public static DeepEqualityResult DeeplyEquals<T>(this T expected, T actual,
            Func<Strategies<T>, Strategies<T>> strategies,
            ComparatorOptions options, params string[] ignore)
        {
            var ignoreStrategy = ComparisonHelper.CreateIgnoreStrategy(ignore, 
                ComparisonHelper.GetIgnoreTypeName(expected, actual, typeof(T), options));
            var customStrategies = strategies(new Strategies<T>());
            return DeeplyEquals(expected, actual, customStrategies.ToDictionary(x => x.Key, x => x.Value),
                ignoreStrategy, options);
        }

        #endregion

        #region Comparisons for Different Types with Strategies

        /// <summary>
        /// Performs a deep equality comparison between objects of different types with custom strategies.
        /// Use this when comparing DTOs to entities or similar scenarios where types differ but share property names.
        /// Strategies are defined based on the expected type (T).
        /// </summary>
        /// <typeparam name="T">The type of the expected object (used for strategy definitions).</typeparam>
        /// <typeparam name="TActual">The type of the actual object.</typeparam>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object to compare against expected.</param>
        /// <param name="strategies">A function to configure custom comparison strategies based on the expected type.</param>
        /// <param name="optionsBuilder">An action to configure comparison options (typically to call AllowDifferentTypes()).</param>
        /// <param name="ignore">Property names to exclude from comparison.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        /// <example>
        /// <code>
        /// // Compare DTO to Entity with custom string handling and ignored properties
        /// var result = expectedDto.DeeplyEquals(actualEntity,
        ///     strategy => strategy
        ///         .Set(x => x.Name, (exp, act) => 
        ///             (string.IsNullOrEmpty(exp) &amp;&amp; string.IsNullOrEmpty(act)) || exp == act)
        ///         .Set(x => x.Description, (exp, act) => 
        ///             (string.IsNullOrEmpty(exp) &amp;&amp; string.IsNullOrEmpty(act)) || exp == act),
        ///     options => options.AllowDifferentTypes(),
        ///     "Id", "CreatedDate");
        /// </code>
        /// </example>
        public static DeepEqualityResult DeeplyEquals<T, TActual>(this T expected, TActual actual,
            Func<Strategies<T>, Strategies<T>> strategies,
            Action<ComparatorOptions> optionsBuilder,
            params string[] ignore)
        {
            var options = ComparisonHelper.BuildOptions(optionsBuilder);
            var ignoreStrategy = ComparisonHelper.CreateIgnoreStrategy(ignore,
                ComparisonHelper.GetIgnoreTypeName(expected, actual, typeof(T), options));
            var customStrategies = strategies(new Strategies<T>());
            return DeeplyEquals<object?>(expected, actual, customStrategies.ToDictionary(x => x.Key, x => x.Value),
                ignoreStrategy, options);
        }

        #endregion

        #region Comparisons with Custom Ignore Strategy

        /// <summary>
        /// Performs a deep equality comparison with a custom ignore strategy function.
        /// </summary>
        /// <typeparam name="T">The type of objects to compare.</typeparam>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object to compare against expected.</param>
        /// <param name="ignoreStrategy">A function that returns true for property paths that should be ignored.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        /// <example>
        /// <code>
        /// // Ignore all properties ending with "Id"
        /// var result = expected.DeeplyEquals(actual, 
        ///     propertyPath => propertyPath.EndsWith("Id"));
        /// </code>
        /// </example>
        public static DeepEqualityResult DeeplyEquals<T>(this T expected, T actual, Func<string, bool> ignoreStrategy) =>
            DeeplyEquals(expected, actual, new Dictionary<string, ICustomCompareValues>(), ignoreStrategy);

        /// <summary>
        /// Performs a deep equality comparison with a custom ignore strategy and options.
        /// </summary>
        /// <typeparam name="T">The type of objects to compare.</typeparam>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object to compare against expected.</param>
        /// <param name="ignoreStrategy">A function that returns true for property paths that should be ignored.</param>
        /// <param name="options">The comparison options.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        public static DeepEqualityResult DeeplyEquals<T>(this T expected, T actual, Func<string, bool> ignoreStrategy,
            ComparatorOptions options) =>
            DeeplyEquals(expected, actual, new Dictionary<string, ICustomCompareValues>(), ignoreStrategy, options);

        #endregion

        #region Internal Core Method

        /// <summary>
        /// Core comparison method that all other overloads delegate to.
        /// </summary>
        internal static DeepEqualityResult DeeplyEquals<T>(T expected, T actual,
            Dictionary<string, ICustomCompareValues> custom, Func<string, bool> ignoreStrategy,
            ComparatorOptions? options = null)
        {
            var compareTypes = new Comparator(custom, ignoreStrategy, options ?? new ComparatorOptions());
            return DeeplyEquals(expected, actual, compareTypes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DeepEqualityResult DeeplyEquals<T>(T expected, T actual, Comparator compareObject) =>
            compareObject.Compare(expected, actual);

        #endregion
    }
}
