using Newtonsoft.Json;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections;

public class DictionaryCompareStrategy : BaseCollectionsCompareStrategy
{
    private static readonly MethodInfo CompareMethod = typeof(DictionaryCompareStrategy).GetTypeInfo()
        .GetDeclaredMethod(nameof(CompareDictionary))!;

    private static readonly Type DictionaryType = typeof(IDictionary<,>);

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
        if (key is null) return "null";

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
        var genericArguments = GetGenericArguments(expected?.GetType() ?? typeof(T));
        return (DeepEqualityResult)CompareMethod.MakeGenericMethod(genericArguments)
            .Invoke(this, new[] { (object)expected, actual, propertyName })!;
    }

    private static Type[] GetGenericArguments(Type type)
    {
        return type.GetInterfaces().Prepend(type)
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == DictionaryType)
            .Select(i => i.GetGenericArguments()).First();
    }

    public override bool IsValid(Type member)
    {
        if (member.IsGenericType && member.GetGenericTypeDefinition() == DictionaryType) return true;

        return member.GetInterfaces().Any(interfaceType =>
            interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == DictionaryType);
    }
}
