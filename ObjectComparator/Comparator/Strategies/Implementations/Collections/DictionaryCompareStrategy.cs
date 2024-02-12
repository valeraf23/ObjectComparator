using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectsComparator.Comparator.RepresentationDistinction;

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
        if (expected.Count != actual.Count)
        {
            const string basePath = "'Dictionary has different length'";
            var path = string.IsNullOrEmpty(propertyName) ? basePath :propertyName + ": " + basePath;
            return DeepEqualityResult.Create(path, expected.Count, actual.Count);
        }

        var diff = DeepEqualityResult.Create();
        foreach (var (key, value) in expected) 
        {
            if (!actual.TryGetValue(key, out var secondValue))
                diff.Add(new Distinction(key.ToString()!, "Should be", "Does not exist"));

            var diffRes = RulesHandler.GetFor(typeof(TValue))
                .Compare(value, secondValue, $"{propertyName}[{key}]");

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