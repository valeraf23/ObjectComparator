using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties;
using ObjectsComparator.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

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
            var ignoreStrategy = CreateIgnoreStrategy(ignore, typeof(T).ToFriendlyTypeName());
            return DeeplyEquals(expected, actual, new Dictionary<string, ICustomCompareValues>(),
                ignoreStrategy, new ComparatorOptions());
        }

        #endregion

        #region Comparisons with Options (Same and Different Types)

        /// <summary>
        /// Performs a deep equality comparison with configurable options.
        /// Supports comparing objects of different types when <see cref="ComparatorOptions.AllowDifferentTypes"/> is enabled.
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <typeparam name="TActual">The type of the actual object.</typeparam>
        /// <param name="expected">The expected object.</param>
        /// <param name="actual">The actual object to compare against expected.</param>
        /// <param name="optionsBuilder">An action to configure comparison options.</param>
        /// <param name="ignore">Property names to exclude from comparison.</param>
        /// <returns>A <see cref="DeepEqualityResult"/> containing any differences found.</returns>
        /// <example>
        /// <code>
        /// // Compare objects of different types, ignoring type mismatches
        /// var result = expected.DeeplyEquals(actual, 
        ///     options => options.AllowDifferentTypes(), 
        ///     "Id", "Timestamp");
        /// </code>
        /// </example>
        public static DeepEqualityResult DeeplyEquals<T, TActual>(this T expected, TActual actual,
            Action<ComparatorOptions> optionsBuilder, params string[] ignore)
        {
            var options = new ComparatorOptions();
            optionsBuilder?.Invoke(options);

            var ignoreStrategy = CreateIgnoreStrategy(ignore,
                GetIgnoreTypeName(expected, actual, typeof(T), options));

            return DeeplyEquals<object?>(expected, actual, new Dictionary<string, ICustomCompareValues>(),
                ignoreStrategy, options);
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
            var ignoreStrategy = CreateIgnoreStrategy(ignore, typeof(T).ToFriendlyTypeName());
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
            var ignoreStrategy = CreateIgnoreStrategy(ignore, typeof(T).ToFriendlyTypeName());
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
            var ignoreStrategy = CreateIgnoreStrategy(ignore, typeof(T).ToFriendlyTypeName());
            return DeeplyEquals(expected, actual, strategies.ToDictionary(x => x.Key, x => x.Value),
                ignoreStrategy);
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
            var options = new ComparatorOptions();
            optionsBuilder?.Invoke(options);

            var ignoreStrategy = CreateIgnoreStrategy(ignore, GetIgnoreTypeName(expected, actual, typeof(T), options));
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
            var ignoreStrategy = CreateIgnoreStrategy(ignore, GetIgnoreTypeName(expected, actual, typeof(T), options));
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
            var ignoreStrategy = CreateIgnoreStrategy(ignore, GetIgnoreTypeName(expected, actual, typeof(T), options));
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
            var options = new ComparatorOptions();
            optionsBuilder?.Invoke(options);

            var ignoreStrategy = CreateIgnoreStrategy(ignore,
                GetIgnoreTypeName(expected, actual, typeof(T), options));
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

        #region Private Helpers

        /// <summary>
        /// Converts property names to fully qualified paths by prefixing with the type name.
        /// </summary>
        /// <param name="ignore">Property names to convert.</param>
        /// <param name="typeName">The type name to use as prefix.</param>
        /// <returns>A set of fully qualified property paths.</returns>
        private static HashSet<string> ConvertToFullPath(IEnumerable<string> ignore, string typeName)
        {
            var prefix = $"{typeName}.";
            var set = new HashSet<string>(StringComparer.Ordinal);
            foreach (var i in ignore ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(i)) continue;
                if (i.StartsWith(prefix, StringComparison.Ordinal))
                {
                    set.Add(i);
                }
                else
                {
                    set.Add(prefix + i);
                }
            }

            return set;
        }

        /// <summary>
        /// Creates a function that determines whether a property should be ignored during comparison.
        /// Handles both simple property paths and paths containing collection indexers.
        /// </summary>
        /// <param name="ignore">Property names to ignore.</param>
        /// <param name="typeName">The type name for path resolution.</param>
        /// <returns>A function that returns true if the property should be ignored.</returns>
        private static Func<string, bool> CreateIgnoreStrategy(IEnumerable<string> ignore, string typeName)
        {
            var ignoreFullPath = ConvertToFullPath(ignore, typeName);
            return propertyName =>
            {
                if (string.IsNullOrEmpty(propertyName))
                {
                    return false;
                }

                if (ignoreFullPath.Contains(propertyName))
                {
                    return true;
                }

                if (propertyName.IndexOf('[', StringComparison.Ordinal) < 0)
                {
                    return false;
                }

                var normalizedPath = RemoveIndexerSegments(propertyName);
                return ignoreFullPath.Contains(normalizedPath);
            };
        }

        /// <summary>
        /// Removes indexer segments (e.g., "[0]", "[key]") from a property path.
        /// For example, "Items[0].Name" becomes "Items.Name".
        /// </summary>
        /// <param name="propertyName">The property path to normalize.</param>
        /// <returns>The property path without indexer segments.</returns>
        private static string RemoveIndexerSegments(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return propertyName;
            }

            var indexStart = propertyName.IndexOf('[', StringComparison.Ordinal);
            if (indexStart < 0)
            {
                return propertyName;
            }

            var builder = new StringBuilder(propertyName.Length);
            var insideIndexer = false;

            foreach (var ch in propertyName)
            {
                if (insideIndexer)
                {
                    if (ch == ']')
                    {
                        insideIndexer = false;
                    }

                    continue;
                }

                if (ch == '[')
                {
                    insideIndexer = true;
                    continue;
                }

                builder.Append(ch);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Determines the type name to use for ignore path resolution.
        /// When comparing different types, uses the actual runtime type instead of the generic type parameter.
        /// </summary>
        private static string GetIgnoreTypeName(object? expected, object? actual, Type defaultType,
            ComparatorOptions? options)
        {
            if (options?.DifferentTypesAllowed == true)
            {
                if (expected != null) return expected.GetType().ToFriendlyTypeName();
                if (actual != null) return actual.GetType().ToFriendlyTypeName();
            }

            return defaultType.ToFriendlyTypeName();
        }

        #endregion
    }
}
