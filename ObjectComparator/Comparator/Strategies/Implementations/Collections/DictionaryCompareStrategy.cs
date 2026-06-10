using Newtonsoft.Json;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Helpers.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections;

internal sealed class DictionaryCompareStrategy : BaseCollectionsCompareStrategy
{
    private static readonly MethodInfo CompareMethod = typeof(DictionaryCompareStrategy).GetTypeInfo()
        .GetDeclaredMethod(nameof(CompareDictionary))!;

    private static readonly Type DictionaryType = typeof(IDictionary<,>);
    private static readonly ConcurrentDictionary<Type, (PropertyInfo? Key, PropertyInfo? Value)> EntryAccessors = new();
    private static readonly ConcurrentDictionary<Type, CompareDictionaryDelegate> CompareDelegates = new();

    public DictionaryCompareStrategy(Comparator comparator) : base(comparator)
    {
    }

    public DeepEqualityResult CompareDictionary<TKey, TValue>(IDictionary<TKey, TValue> expected,
        IDictionary<TKey, TValue> actual, string propertyName) where TKey : notnull
    {
        var diff = DeepEqualityResult.None();
        var commonDiff = DeepEqualityResult.None();

        List<KeyValuePair<TKey, TValue>>? removedEntries = null;
        List<KeyValuePair<TKey, TValue>>? addedEntries = null;

        var valueComparer = RulesHandler.GetFor(typeof(TValue));

        foreach (var pair in expected)
        {
            if (actual.TryGetValue(pair.Key, out var actualValue))
            {
                var diffRes = valueComparer.Compare(pair.Value, actualValue,
                    $"{propertyName}[{FormatKey(pair.Key)}]");
                commonDiff.AddRange(diffRes);
            }
            else
            {
                (removedEntries ??= new List<KeyValuePair<TKey, TValue>>()).Add(pair);
            }
        }

        var commonCount = expected.Count - (removedEntries?.Count ?? 0);
        if (actual.Count > commonCount)
        {
            foreach (var pair in actual)
            {
                if (!expected.ContainsKey(pair.Key))
                {
                    (addedEntries ??= new List<KeyValuePair<TKey, TValue>>()).Add(pair);
                }
            }
        }

        var keyType = typeof(TKey);

        if (keyType == typeof(string) || keyType.IsPrimitive || keyType.IsEnum || keyType.IsToStringOverridden())
        {
            if (addedEntries is not null)
            {
                diff.Add(new Distinction(propertyName, null,
                    string.Join(", ", addedEntries.Select(pair => pair.Key)), "Added"));
            }

            if (removedEntries is not null)
            {
                diff.Add(new Distinction(propertyName,
                    string.Join(", ", removedEntries.Select(pair => pair.Key)), null, "Removed"));
            }
        }
        else
        {
            if (removedEntries is not null)
            {
                foreach (var pair in removedEntries)
                {
                    diff.Add(new Distinction(
                        $"{propertyName}[{FormatKey(pair.Key)}]",
                        FormatValue(pair.Value),
                        null,
                        "Removed"));
                }
            }

            if (addedEntries is not null)
            {
                foreach (var pair in addedEntries)
                {
                    diff.Add(new Distinction(
                        $"{propertyName}[{FormatKey(pair.Key)}]",
                        null,
                        FormatValue(pair.Value),
                        "Added"));
                }
            }
        }

        return diff.AddRange(commonDiff);
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

        var compareDelegate = CompareDelegates.GetOrAdd(expectedType, CreateCompareDelegate);
        return compareDelegate(this, expected!, actual!, propertyName);
    }

    private static CompareDictionaryDelegate CreateCompareDelegate(Type dictionaryType)
    {
        var genericArguments = GetGenericArguments(dictionaryType);
        var method = CompareMethod.MakeGenericMethod(genericArguments);
        var interfaceType = DictionaryType.MakeGenericType(genericArguments);

        var selfParameter = Expression.Parameter(typeof(DictionaryCompareStrategy), "self");
        var expectedParameter = Expression.Parameter(typeof(object), "expected");
        var actualParameter = Expression.Parameter(typeof(object), "actual");
        var propertyNameParameter = Expression.Parameter(typeof(string), "propertyName");

        var call = Expression.Call(selfParameter, method,
            Expression.Convert(expectedParameter, interfaceType),
            Expression.Convert(actualParameter, interfaceType),
            propertyNameParameter);

        return Expression.Lambda<CompareDictionaryDelegate>(call, selfParameter, expectedParameter,
            actualParameter, propertyNameParameter).Compile();
    }

    private delegate DeepEqualityResult CompareDictionaryDelegate(DictionaryCompareStrategy self, object expected,
        object actual, string propertyName);

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

        var commonDiff = DeepEqualityResult.None();

        foreach (var pair in expectedEntries)
        {
            if (actualEntries.TryGetValue(pair.Key, out var actualValue))
            {
                var expectedValue = pair.Value;
                var expectedValueRuntimeType = expectedValue?.GetType() ?? expectedValueType;
                var actualValueRuntimeType = actualValue?.GetType() ?? actualValueType;

                var diffRes = Comparator.CompareWithTypes(expectedValue, actualValue,
                    $"{propertyName}[{FormatKey(pair.Key)}]", expectedValueRuntimeType, actualValueRuntimeType);

                commonDiff.AddRange(diffRes);
            }
            else
            {
                diff.Add(new Distinction(
                    $"{propertyName}[{FormatKey(pair.Key)}]",
                    FormatValue(pair.Value),
                    null,
                    "Removed"));
            }
        }

        foreach (var pair in actualEntries)
        {
            if (!expectedEntries.ContainsKey(pair.Key))
            {
                diff.Add(new Distinction(
                    $"{propertyName}[{FormatKey(pair.Key)}]",
                    null,
                    FormatValue(pair.Value),
                    "Added"));
            }
        }

        return diff.AddRange(commonDiff);
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
        return member.IsGenericDictionary();
    }
}