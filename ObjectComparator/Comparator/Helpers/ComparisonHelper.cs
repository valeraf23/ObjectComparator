using ObjectsComparator.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectsComparator.Comparator.Helpers;

internal static class ComparisonHelper
{
    internal static ComparatorOptions BuildOptions(Action<ComparatorOptions>? optionsBuilder)
    {
        var options = new ComparatorOptions();
        optionsBuilder?.Invoke(options);
        return options;
    }

    internal static HashSet<string> ConvertToFullPath(IEnumerable<string> ignore, string typeName)
    {
        var prefix = $"{typeName}.";
        var set = new HashSet<string>(StringComparer.Ordinal);

        string? elementTypePrefix = null;
        var genericStart = typeName.IndexOf('<');
        if (genericStart >= 0)
        {
            var genericEnd = typeName.IndexOf('>', genericStart);
            if (genericEnd > genericStart)
            {
                var elementTypeName = typeName.Substring(genericStart + 1, genericEnd - genericStart - 1);
                elementTypePrefix = $"{elementTypeName}.";
            }
        }

        foreach (var i in ignore ?? [])
        {
            if (string.IsNullOrWhiteSpace(i))
            {
                continue;
            }

            if (i.StartsWith(prefix, StringComparison.Ordinal))
            {
                set.Add(i);
            }
            else
            {
                set.Add(prefix + i);

                if (elementTypePrefix != null)
                {
                    set.Add(elementTypePrefix + i);
                }
            }
        }

        return set;
    }

    internal static Func<string, bool> CreateIgnoreStrategy(IEnumerable<string> ignore, string typeName)
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

    internal static string RemoveIndexerSegments(string propertyName)
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

        var result = builder.ToString();

        var genericStart = result.IndexOf('<');
        if (genericStart < 0)
        {
            return result;
        }

        var genericEnd = result.IndexOf('>', genericStart);
        if (genericEnd <= genericStart)
        {
            return result;
        }

        var afterGeneric = genericEnd + 1;
        if (afterGeneric >= result.Length || result[afterGeneric] != '.')
        {
            return result;
        }

        var elementTypeName = result.Substring(genericStart + 1, genericEnd - genericStart - 1);

        var propertyStart = afterGeneric + 1;

        while (propertyStart < result.Length && result[propertyStart] == '.')
        {
            propertyStart++;
        }

        if (propertyStart >= result.Length)
        {
            return result;
        }

        var propertyPart = result[propertyStart..];
        return elementTypeName + "." + propertyPart;
    }

    internal static string GetIgnoreTypeName(object? expected, object? actual, Type defaultType,
        ComparatorOptions? options)
    {
        if (options?.DifferentTypesAllowed == true)
        {
            if (expected != null)
            {
                return expected.GetType().ToFriendlyTypeName();
            }

            if (actual != null)
            {
                return actual.GetType().ToFriendlyTypeName();
            }
        }

        return defaultType.ToFriendlyTypeName();
    }
}