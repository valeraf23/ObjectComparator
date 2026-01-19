using System;
using System.Buffers;

namespace ObjectsComparator.Comparator.Rules;

/// <summary>
///     Normalizes property paths by removing collection indexers and extracting element types from generics.
///     Example: "List<Person>[0].Name" becomes "Person.Name"
/// </summary>
public static class PropertyPathNormalizer
{
    private const int StackAllocThreshold = 256;

    public static string Normalize(string propertyPath)
    {
        if (string.IsNullOrEmpty(propertyPath))
        {
            return propertyPath;
        }

        var pathWithoutIndexers = RemoveIndexers(propertyPath);

        return ContainsGenericType(pathWithoutIndexers)
            ? ExtractElementTypePath(pathWithoutIndexers)
            : pathWithoutIndexers;
    }

    public static bool ContainsIndexer(string propertyPath)
    {
        return propertyPath.AsSpan().IndexOf('[') >= 0;
    }

    private static bool ContainsGenericType(string path)
    {
        return path.AsSpan().IndexOf('<') >= 0;
    }

    private static string RemoveIndexers(string propertyPath)
    {
        var source = propertyPath.AsSpan();
        char[]? rentedBuffer = null;

        var buffer = source.Length <= StackAllocThreshold
            ? stackalloc char[source.Length]
            : rentedBuffer = ArrayPool<char>.Shared.Rent(source.Length);

        try
        {
            var writePosition = CopyWithoutIndexers(source, buffer);
            return new string(buffer[..writePosition]);
        }
        finally
        {
            if (rentedBuffer is not null)
            {
                ArrayPool<char>.Shared.Return(rentedBuffer);
            }
        }
    }

    private static int CopyWithoutIndexers(ReadOnlySpan<char> source, Span<char> destination)
    {
        var writePosition = 0;
        var insideIndexer = false;

        foreach (var character in source)
        {
            if (insideIndexer)
            {
                if (character == ']')
                {
                    insideIndexer = false;
                }

                continue;
            }

            if (character == '[')
            {
                insideIndexer = true;
                continue;
            }

            destination[writePosition++] = character;
        }

        return writePosition;
    }

    private static string ExtractElementTypePath(string genericPath)
    {
        var span = genericPath.AsSpan();

        var genericOpenIndex = span.IndexOf('<');
        if (genericOpenIndex < 0)
        {
            return genericPath;
        }

        var genericCloseIndex = FindMatchingCloseBracket(span, genericOpenIndex);
        if (genericCloseIndex < 0 || !TryGetPropertyStartIndex(span, genericCloseIndex, out var propertyStartIndex))
        {
            return genericPath;
        }

        return BuildElementTypePath(genericPath, genericOpenIndex, genericCloseIndex, propertyStartIndex);
    }

    private static int FindMatchingCloseBracket(ReadOnlySpan<char> span, int openIndex)
    {
        var depth = 0;

        for (var i = openIndex; i < span.Length; i++)
        {
            switch (span[i])
            {
                case '<':
                    depth++;
                    break;
                case '>' when --depth == 0:
                    return i;
            }
        }

        return -1;
    }

    private static bool TryGetPropertyStartIndex(ReadOnlySpan<char> span, int genericCloseIndex,
        out int propertyStartIndex)
    {
        var dotIndex = genericCloseIndex + 1;

        if (dotIndex >= span.Length || span[dotIndex] != '.')
        {
            propertyStartIndex = -1;
            return false;
        }

        propertyStartIndex = SkipDots(span, dotIndex + 1);
        return propertyStartIndex < span.Length;
    }

    private static int SkipDots(ReadOnlySpan<char> span, int startIndex)
    {
        var index = startIndex;
        while (index < span.Length && span[index] == '.')
        {
            index++;
        }

        return index;
    }

    private static string BuildElementTypePath(
        string source,
        int genericOpenIndex,
        int genericCloseIndex,
        int propertyStartIndex)
    {
        var elementTypeStart = genericOpenIndex + 1;
        var elementTypeLength = genericCloseIndex - elementTypeStart;
        var propertyLength = source.Length - propertyStartIndex;
        var totalLength = elementTypeLength + 1 + propertyLength;

        return string.Create(totalLength,
            (source, elementTypeStart, elementTypeLength, propertyStartIndex),
            static (destination, state) =>
            {
                var (src, elemStart, elemLen, propStart) = state;

                src.AsSpan(elemStart, elemLen).CopyTo(destination);
                destination[elemLen] = '.';
                src.AsSpan(propStart).CopyTo(destination[(elemLen + 1)..]);
            });
    }
}