using Newtonsoft.Json;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Rules;
using ObjectsComparator.Helpers.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections;

internal sealed class CollectionsCompareStrategy : BaseCollectionsCompareStrategy
{
    private static readonly MethodInfo CompareCollectionsMethod =
        typeof(CollectionsCompareStrategy).GetTypeInfo().GetDeclaredMethod(nameof(CompareCollections))!;

    private static readonly Type ListType = typeof(IEnumerable<>);

    private static readonly ConcurrentDictionary<Type, Type> GenericArgumentCache = new();

    private static readonly ConcurrentDictionary<Type, Func<object, object, string, RulesHandler, DeepEqualityResult>>
        CompareDelegates = new();

    public CollectionsCompareStrategy(Comparator comparator) : base(comparator)
    {
    }

    private static string GetIndexedPropertyName(string propertyName, int i)
    {
        return i < 10
            ? $"{propertyName}[{(char)('0' + i)}]"
            : $"{propertyName}[{i}]";
    }

    public override bool IsValid(Type member)
    {
        return member.IsGenericEnumerable();
    }

    public override DeepEqualityResult Compare<T>(T expected, T actual, string propertyName)
    {
        var expectedType = expected?.GetType() ?? typeof(T);
        var actualType = actual?.GetType() ?? typeof(T);

        if (Comparator.Options.DifferentTypesAllowed && expectedType != actualType)
        {
            var expectedHasElementType = TryGetGenericArgument(expectedType, out var expectedElementType);
            var actualHasElementType = TryGetGenericArgument(actualType, out var actualElementType);

            if (!expectedHasElementType || !actualHasElementType
                                        || expectedElementType != actualElementType)
            {
                return CompareDifferentTypes(expected, actual, propertyName);
            }
        }

        var genericType = GetGenericArgument(expectedType);
        var compareDelegate = CompareDelegates.GetOrAdd(genericType,
            static elementType => CollectionHelper.GetDelegateFor(
                CompareCollectionsMethod.MakeGenericMethod(elementType)));
        return compareDelegate(expected!, actual!, propertyName, RulesHandler);
    }

    private static Type GetGenericArgument(Type type)
    {
        return GenericArgumentCache.GetOrAdd(type, static t =>
            t.GetInterfaces().Prepend(t)
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == ListType)
                .Select(i => i.GetGenericArguments()[0]).First());
    }

    private static bool TryGetGenericArgument(Type type, out Type genericArgument)
    {
        var interfaceType = type.GetInterfaces().Prepend(type)
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == ListType);

        if (interfaceType is null)
        {
            genericArgument = typeof(object);
            return false;
        }

        genericArgument = interfaceType.GetGenericArguments()[0];
        return true;
    }

    private static object GetFormattedValue<T>(T item, bool isPrimitive, bool couldToString)
    {
        return isPrimitive ? item : couldToString ? item?.ToString() : FormatValue(item);
    }

    private static void AddExtraDifferences<T>(
        IReadOnlyList<T> source,
        int startIndex,
        string propertyName,
        string action,
        bool isPrimitive,
        bool couldToString,
        DeepEqualityResult diff)
    {
        for (var i = startIndex; i < source.Count; i++)
        {
            var indexedName = GetIndexedPropertyName(propertyName, i);
            var value = GetFormattedValue(source[i], isPrimitive, couldToString);
            if (action == "Removed")
            {
                diff.Add(new Distinction(indexedName, value, null, action));
            }
            else if (action == "Added")
            {
                diff.Add(new Distinction(indexedName, null, value, action));
            }
        }
    }

    private static DeepEqualityResult CompareLists<T>(
        IReadOnlyList<T> expected,
        IReadOnlyList<T> actual,
        string propertyName,
        RulesHandler rulesHandler)
    {
        var type = typeof(T);
        var isPrimitive = type == typeof(string) || type.IsPrimitive;
        var couldToString = type.IsEnum || type.IsToStringOverridden();
        var diff = DeepEqualityResult.None();
        var minCount = Math.Min(expected.Count, actual.Count);
        var handler = rulesHandler.GetFor(type);

        for (var i = 0; i < minCount; i++)
        {
            var indexedName = GetIndexedPropertyName(propertyName, i);
            var elementDiff = handler.Compare(expected[i], actual[i], indexedName);
            diff.AddRange(elementDiff);
        }

        if (expected.Count > actual.Count)
        {
            AddExtraDifferences(expected, actual.Count, propertyName, "Removed", isPrimitive, couldToString, diff);
        }
        else if (actual.Count > expected.Count)
        {
            AddExtraDifferences(actual, expected.Count, propertyName, "Added", isPrimitive, couldToString, diff);
        }

        return diff;
    }

    private static string FormatValue<T>(T value)
    {
        return JsonConvert.SerializeObject(value, SerializerSettings.Settings);
    }

    private static object? FormatValue(object? value)
    {
        if (value is null)
        {
            return null;
        }

        var type = value.GetType();
        if (type == typeof(string) || type.IsPrimitive || type.IsEnum || type.IsToStringOverridden())
        {
            return value;
        }

        return JsonConvert.SerializeObject(value, SerializerSettings.Settings);
    }

    private static DeepEqualityResult CompareCollections<T>(
        IEnumerable<T> expected,
        IEnumerable<T> actual,
        string propertyName,
        RulesHandler rulesHandler)
    {
        var exp = EnsureReadOnlyList(expected);
        var act = EnsureReadOnlyList(actual);
        return CompareLists(exp, act, propertyName, rulesHandler);
    }

    private static IReadOnlyList<T> EnsureReadOnlyList<T>(IEnumerable<T> source)
    {
        if (source is null)
        {
            return Array.Empty<T>();
        }

        switch (source)
        {
            case IReadOnlyList<T> readOnlyList:
                return readOnlyList;
            case ICollection<T> collection:
            {
                var array = new T[collection.Count];
                collection.CopyTo(array, 0);
                return array;
            }
            default:
                return source.ToList();
        }
    }

    internal DeepEqualityResult CompareDifferentTypes(object expected, object actual, string propertyName)
    {
        if (expected is not IEnumerable expectedEnumerable || actual is not IEnumerable actualEnumerable)
        {
            return DeepEqualityResult.Create(propertyName, expected, actual);
        }

        var exp = EnsureReadOnlyList(expectedEnumerable);
        var act = EnsureReadOnlyList(actualEnumerable);

        var expectedType = expected.GetType();
        var actualType = actual.GetType();

        var expectedElementType = TryGetGenericArgument(expectedType, out var expType) ? expType : typeof(object);
        var actualElementType = TryGetGenericArgument(actualType, out var actType) ? actType : typeof(object);

        return CompareObjectLists(exp, act, propertyName, expectedElementType, actualElementType);
    }

    private DeepEqualityResult CompareObjectLists(IReadOnlyList<object?> expected, IReadOnlyList<object?> actual,
        string propertyName, Type expectedElementType, Type actualElementType)
    {
        var diff = DeepEqualityResult.None();
        var minCount = Math.Min(expected.Count, actual.Count);

        for (var i = 0; i < minCount; i++)
        {
            var expectedValue = expected[i];
            var actualValue = actual[i];

            var expectedValueType = expectedValue?.GetType() ?? expectedElementType;
            var actualValueType = actualValue?.GetType() ?? actualElementType;

            var indexedName = GetIndexedPropertyName(propertyName, i);
            var elementDiff = Comparator.CompareWithTypes(expectedValue, actualValue, indexedName,
                expectedValueType, actualValueType);

            diff.AddRange(elementDiff);
        }

        if (expected.Count > actual.Count)
        {
            AddExtraDifferences(expected, actual.Count, propertyName, "Removed", diff);
        }
        else if (actual.Count > expected.Count)
        {
            AddExtraDifferences(actual, expected.Count, propertyName, "Added", diff);
        }

        return diff;
    }

    private static void AddExtraDifferences(
        IReadOnlyList<object?> source,
        int startIndex,
        string propertyName,
        string action,
        DeepEqualityResult diff)
    {
        for (var i = startIndex; i < source.Count; i++)
        {
            var indexedName = GetIndexedPropertyName(propertyName, i);
            var value = FormatValue(source[i]);
            if (action == "Removed")
            {
                diff.Add(new Distinction(indexedName, value, null, action));
            }
            else if (action == "Added")
            {
                diff.Add(new Distinction(indexedName, null, value, action));
            }
        }
    }

    private static IReadOnlyList<object?> EnsureReadOnlyList(IEnumerable source)
    {
        if (source is null)
        {
            return Array.Empty<object?>();
        }

        if (source is ICollection collection)
        {
            var array = new object?[collection.Count];
            collection.CopyTo(array, 0);
            return array;
        }

        return source.Cast<object?>().ToList();
    }
}