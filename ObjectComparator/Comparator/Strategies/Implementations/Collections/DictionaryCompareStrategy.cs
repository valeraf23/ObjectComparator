using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Helpers.Extensions;

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
        var addedKeys = actual.Keys.Except(expected.Keys).ToList();
        var removedKeys = expected.Keys.Except(actual.Keys).ToList();
       
        var keyType = typeof(TKey);

        if (keyType == typeof(string) || keyType.IsPrimitive || keyType.IsToStringOverridden())
        {
            if (addedKeys.Count > 0)
            {
                diff.Add(new Distinction($"{propertyName}", null, string.Join(", ", addedKeys), "Added"));
            }

            if (removedKeys.Count > 0)
            {
                diff.Add(new Distinction($"{propertyName}", string.Join(", ", removedKeys), null,  "Removed"));
            }
        }
        else
        {
            const string basePath = "'Dictionary has different length'";
            diff.Add(new Distinction($"{propertyName}", expected.Count, actual.Count,  basePath));
        }

        var commonKeys = expected.Keys.Intersect(actual.Keys);

        foreach (var key in commonKeys) 
        {
            var expectedValue = expected[key];
            var actualValue = actual[key];

            var diffRes = RulesHandler.GetFor(typeof(TValue))
                .Compare(expectedValue, actualValue, $"{propertyName}[{key}]");

            diff.AddRange(diffRes);
        }

        return diff;
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