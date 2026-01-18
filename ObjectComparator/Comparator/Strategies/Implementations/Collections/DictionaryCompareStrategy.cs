using Newtonsoft.Json;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Helpers.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections;

public class DictionaryCompareStrategy : BaseCollectionsCompareStrategy
{
    private static readonly MethodInfo CompareMethod = typeof(DictionaryCompareStrategy).GetTypeInfo()
        .GetDeclaredMethod(nameof(CompareDictionary))!;

    private static readonly Type DictionaryType = typeof(IDictionary<,>);
    private static readonly ConcurrentDictionary<Type, (PropertyInfo? Key, PropertyInfo? Value)> EntryAccessors = new();

    public DictionaryCompareStrategy(Comparator comparator) : base(comparator)
    {
    }

    public DeepEqualityResult CompareDictionary<TKey, TValue>(IDictionary<TKey, TValue> expected,
        IDictionary<TKey, TValue> actual, string propertyName) where TKey : notnull
    {
        var diff = DeepEqualityResult.None();

        var expectedKeys = expected.Keys.ToHashSet();
        var actualKeys = actual.Keys.ToHashSet();

        var addedKeys = actualKeys.Except(expectedKeys).ToList();
        var removedKeys = expectedKeys.Except(actualKeys).ToList();

        var keyType = typeof(TKey);

        if (keyType == typeof(string) || keyType.IsPrimitive || keyType.IsEnum || keyType.IsToStringOverridden())
        {
            if (addedKeys.Count > 0)
            {
                diff.Add(new Distinction($"{propertyName}", null, string.Join(", ", addedKeys), "Added"));
            }

            if (removedKeys.Count > 0)
            {
                diff.Add(new Distinction($"{propertyName}", string.Join(", ", removedKeys), null, "Removed"));
            }
        }
        else
        {
            foreach (var key in removedKeys)
            {
                diff.Add(new Distinction(
                    $"{propertyName}[{FormatKey(key)}]",
                    FormatValue(expected[key]),
                    null,
                    "Removed"));
            }

            foreach (var key in addedKeys)
            {
                diff.Add(new Distinction(
                    $"{propertyName}[{FormatKey(key)}]",
                    null,
                    FormatValue(actual[key]),
                    "Added"));
            }
        }

        var commonKeys = expectedKeys.Intersect(actualKeys);

        foreach (var key in commonKeys)
        {
            var expectedValue = expected[key];
            var actualValue = actual[key];

            var diffRes = RulesHandler.GetFor(typeof(TValue))
                .Compare(expectedValue, actualValue, $"{propertyName}[{FormatKey(key)}]");

            diff.AddRange(diffRes);
        }

        return diff;
    }

    private static string FormatKey<TKey>(TKey key)
    {
        if (key is null)
        {
            return "null";
        }

        var type = key.GetType();
        if (type == typeof(string) || type.IsPrimitive || type.IsEnum || type.IsToStringOverridden())
        {
            return key.ToString()!;
        }

        try
        {
            return JsonConvert.SerializeObject(key, SerializerSettings.Settings);
        }
        catch
        {
            return key.GetHashCode().ToString();
        }
    }

    private static object? FormatValue<TValue>(TValue value)
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

    public override DeepEqualityResult Compare<T>(T expected, T actual, string propertyName)
    {
        var expectedType = expected?.GetType() ?? typeof(T);
        var actualType = actual?.GetType() ?? typeof(T);

        if (Comparator.Options.DifferentTypesAllowed && expectedType != actualType)
        {
            if (!TryGetDictionaryArguments(expectedType, out var expectedKeyType, out var expectedValueType)
                || !TryGetDictionaryArguments(actualType, out var actualKeyType, out var actualValueType)
                || expectedKeyType != actualKeyType
                || expectedValueType != actualValueType)
            {
                return CompareDifferentTypes(expected!, actual!, propertyName);
            }
        }

        var genericArguments = GetGenericArguments(expectedType);
        return (DeepEqualityResult)CompareMethod.MakeGenericMethod(genericArguments)
            .Invoke(this, new[] { (object)expected, actual, propertyName })!;
    }

    private static bool TryGetDictionaryArguments(Type type, out Type keyType, out Type valueType)
    {
        var interfaceType = type.GetInterfaces().Prepend(type)
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == DictionaryType);

        if (interfaceType is null)
        {
            keyType = typeof(object);
            valueType = typeof(object);
            return false;
        }

        var args = interfaceType.GetGenericArguments();
        keyType = args[0];
        valueType = args[1];
        return true;
    }

    private static Type[] GetGenericArguments(Type type)
    {
        return type.GetInterfaces().Prepend(type)
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == DictionaryType)
            .Select(i => i.GetGenericArguments()).First();
    }

    internal DeepEqualityResult CompareDifferentTypes(object expected, object actual, string propertyName)
    {
        if (expected is not IEnumerable expectedEnumerable || actual is not IEnumerable actualEnumerable)
        {
            return DeepEqualityResult.Create(propertyName, expected, actual);
        }

        var expectedEntries = ToObjectDictionary(expectedEnumerable);
        var actualEntries = ToObjectDictionary(actualEnumerable);

        var diff = DeepEqualityResult.None();

        var expectedValueType = TryGetDictionaryArguments(expected.GetType(), out _, out var expValueType)
            ? expValueType
            : typeof(object);
        var actualValueType = TryGetDictionaryArguments(actual.GetType(), out _, out var actValueType)
            ? actValueType
            : typeof(object);

        var expectedKeys = expectedEntries.Keys.ToHashSet();
        var actualKeys = actualEntries.Keys.ToHashSet();

        var addedKeys = actualKeys.Except(expectedKeys).ToList();
        var removedKeys = expectedKeys.Except(actualKeys).ToList();

        foreach (var key in removedKeys)
        {
            diff.Add(new Distinction(
                $"{propertyName}[{FormatKey(key)}]",
                FormatValue(expectedEntries[key]),
                null,
                "Removed"));
        }

        foreach (var key in addedKeys)
        {
            diff.Add(new Distinction(
                $"{propertyName}[{FormatKey(key)}]",
                null,
                FormatValue(actualEntries[key]),
                "Added"));
        }

        var commonKeys = expectedKeys.Intersect(actualKeys);

        foreach (var key in commonKeys)
        {
            var expectedValue = expectedEntries[key];
            var actualValue = actualEntries[key];

            var expectedValueRuntimeType = expectedValue?.GetType() ?? expectedValueType;
            var actualValueRuntimeType = actualValue?.GetType() ?? actualValueType;

            var diffRes = Comparator.CompareWithTypes(expectedValue, actualValue,
                $"{propertyName}[{FormatKey(key)}]", expectedValueRuntimeType, actualValueRuntimeType);

            diff.AddRange(diffRes);
        }

        return diff;
    }

    private static Dictionary<object?, object?> ToObjectDictionary(IEnumerable source)
    {
        var result = new Dictionary<object?, object?>();

        if (source is IDictionary nonGenericDictionary)
        {
            foreach (DictionaryEntry entry in nonGenericDictionary)
            {
                result[entry.Key] = entry.Value;
            }

            return result;
        }

        foreach (var entry in source)
        {
            if (entry is null)
            {
                continue;
            }

            if (!TryGetEntryAccessors(entry.GetType(), out var keyProperty, out var valueProperty))
            {
                continue;
            }

            var key = keyProperty.GetValue(entry);
            var value = valueProperty.GetValue(entry);
            result[key] = value;
        }

        return result;
    }

    private static bool TryGetEntryAccessors(Type entryType, out PropertyInfo keyProperty,
        out PropertyInfo valueProperty)
    {
        var accessors = EntryAccessors.GetOrAdd(entryType, type =>
        {
            var key = type.GetProperty("Key");
            var value = type.GetProperty("Value");
            return (Key: key, Value: value);
        });

        keyProperty = accessors.Key!;
        valueProperty = accessors.Value!;
        return keyProperty is not null && valueProperty is not null;
    }

    public override bool IsValid(Type member)
    {
        if (member.IsGenericType && member.GetGenericTypeDefinition() == DictionaryType)
        {
            return true;
        }

        return member.GetInterfaces().Any(interfaceType =>
            interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == DictionaryType);
    }
}