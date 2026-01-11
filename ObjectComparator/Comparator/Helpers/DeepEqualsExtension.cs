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
    public static class DeepEqualsExtension
    {

        public static string ToJson(this DeepEqualityResult distinctions)
        {
            return DeepComparisonJsonConverter.ToJson(distinctions);
        }

        extension<T>(T expected)
        {
            public DeepEqualityResult DeeplyEquals(T actual, params string[] ignore) =>
                DeeplyEquals(expected, actual, new Strategies<T>(), ignore);

            public DeepEqualityResult DeeplyEquals<TActual>(TActual actual,
                Action<ComparatorOptions> optionsBuilder, params string[] ignore)
            {
                var options = new ComparatorOptions();
                optionsBuilder?.Invoke(options);

                var ignoreStrategy = CreateIgnoreStrategy(ignore,
                    GetIgnoreTypeName(expected, actual, typeof(T), options));

                return DeeplyEquals<object?>(expected, actual, new Dictionary<string, ICustomCompareValues>(),
                    ignoreStrategy, options);
            }

            public DeepEqualityResult DeeplyEqualsIgnoreObjectTypes<TActual>(TActual actual, params string[] ignore) =>
                DeeplyEquals(expected, actual, options => options.AllowDifferentTypes(), ignore);

            public DeepEqualityResult DeeplyEquals(T actual, Strategies<T> custom,
                params string[] ignore)
            {
                var ignoreStrategy = CreateIgnoreStrategy(ignore, typeof(T).ToFriendlyTypeName());
                return DeeplyEquals(expected, actual, custom.ToDictionary(x => x.Key, x => x.Value),
                    ignoreStrategy);
            }

            public DeepEqualityResult DeeplyEquals(T actual,
                Func<Strategies<T>, IEnumerable<KeyValuePair<string, ICustomCompareValues>>> strategies,
                params string[] ignore)
            {
                var ignoreStrategy = CreateIgnoreStrategy(ignore, typeof(T).ToFriendlyTypeName());
                var customStr = strategies(new Strategies<T>());
                return DeeplyEquals(expected, actual, customStr.ToDictionary(x => x.Key, x => x.Value),
                    ignoreStrategy);
            }

            public DeepEqualityResult
                DeeplyEquals(T actual, Func<string, bool> ignoreStrategy) =>
                DeeplyEquals(expected, actual, new Dictionary<string, ICustomCompareValues>(), ignoreStrategy);

            public DeepEqualityResult DeeplyEquals(T actual, Func<string, bool> ignoreStrategy,
                ComparatorOptions options) =>
                DeeplyEquals(expected, actual, new Dictionary<string, ICustomCompareValues>(), ignoreStrategy, options);

            public DeepEqualityResult DeeplyEquals(T actual,
                Func<Strategies<T>, IEnumerable<KeyValuePair<string, ICustomCompareValues>>> strategies,
                ComparatorOptions options, params string[] ignore)
            {
                var ignoreStrategy = CreateIgnoreStrategy(ignore, GetIgnoreTypeName(expected, actual, typeof(T), options));
                var customStr = strategies(new Strategies<T>());
                return DeeplyEquals(expected, actual, customStr.ToDictionary(x => x.Key, x => x.Value),
                    ignoreStrategy, options);
            }

            public DeepEqualityResult DeeplyEquals(T actual, ComparatorOptions options,
                params string[] ignore) =>
                DeeplyEquals(expected, actual, new Strategies<T>(), options, ignore);

            public DeepEqualityResult DeeplyEquals(T actual, Strategies<T> custom,
                ComparatorOptions options, params string[] ignore)
            {
                var ignoreStrategy = CreateIgnoreStrategy(ignore, GetIgnoreTypeName(expected, actual, typeof(T), options));
                return DeeplyEquals(expected, actual, custom.ToDictionary(x => x.Key, x => x.Value),
                    ignoreStrategy, options);
            }
        }

        public static DeepEqualityResult DeeplyEquals<T>(T expected, T actual,
            Dictionary<string, ICustomCompareValues> custom, Func<string, bool> ignoreStrategy,
            ComparatorOptions? options = null)
        {
            var compareTypes = new Comparator(custom, ignoreStrategy, options ?? new ComparatorOptions());
            return DeeplyEquals(expected, actual, compareTypes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DeepEqualityResult DeeplyEquals<T>(T expected, T actual, Comparator compareObject) =>
            compareObject.Compare(expected, actual);

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
    }
}
