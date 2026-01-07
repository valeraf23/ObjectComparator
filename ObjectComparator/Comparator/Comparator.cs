using ObjectsComparator.Comparator.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Rules;
using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies;
using ObjectsComparator.Comparator.Strategies.Implementations;
using ObjectsComparator.Comparator.Strategies.Implementations.Collections;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;

namespace ObjectsComparator.Comparator;

public sealed class Comparator
{
    public readonly RulesHandler RulesHandler;
    private readonly ComparatorOptions _options;
    private readonly CompareMembersStrategy _compareMembersStrategy;
    private readonly CollectionsCompareStrategy _collectionsCompareStrategy;
    private readonly DictionaryCompareStrategy _dictionaryCompareStrategy;

    private static readonly Type DictionaryType = typeof(IDictionary<,>);
    private static readonly Type EnumerableType = typeof(IEnumerable<>);

    private static readonly ConcurrentDictionary<Type, CompareValuesDelegate> CompareValuesDelegates = new();

    internal ComparatorOptions Options => _options;

    public Comparator(Dictionary<string, ICustomCompareValues> customStrategies, Func<string, bool> ignoreStrategy, ComparatorOptions options)
    {
        _options = options ?? new ComparatorOptions();
        _compareMembersStrategy = new CompareMembersStrategy(this);
        _collectionsCompareStrategy = new CollectionsCompareStrategy(this);
        _dictionaryCompareStrategy = new DictionaryCompareStrategy(this);

        var rules = new List<Rule>(6)
        {
            Rule.CreateFor(new ComparePrimitiveTypesStrategy())
        };

        if (_options.IsSkipped(StrategyType.Equality) == false)
        {
            rules.Add(Rule.CreateFor(new EqualityStrategy()));
        }

        if (_options.IsSkipped(StrategyType.OverridesEquals) == false)
        {
            rules.Add(Rule.CreateFor(new OverridesEqualsStrategy()));
        }

        rules.Add(_options.IsSkipped(StrategyType.CompareTo) == false
            ? Rule.CreateFor(new ComparablesStrategy())
            : Rule.CreateFor(new ComparablesStrategy(true)));

        rules.Add(Rule.CreateFor<ICollectionsCompareStrategy>(_collectionsCompareStrategy,
            _dictionaryCompareStrategy));

        rules.Add(Rule.CreateFor(_compareMembersStrategy));

        RulesHandler = new RulesHandler(rules, customStrategies, ignoreStrategy);
    }

    public DeepEqualityResult Compare<T>(T expected, T actual)
    {
        var type = expected?.GetType() ?? typeof(T);
        var actualType = actual?.GetType() ?? typeof(T);
        var typeName = type.ToFriendlyTypeName();

        if (type != actualType)
        {
            if (_options.DifferentTypesAllowed)
            {
                if (expected is null && actual is not null)
                {
                    typeName = actualType.ToFriendlyTypeName();
                }
                else if (expected is not null)
                {
                    typeName = type.ToFriendlyTypeName();
                }
            }
            else
            {
                type = typeof(T);
                typeName = type.ToFriendlyTypeName();
            }
        }

        if (expected != null && actual == null || expected == null && actual != null)
            return DeepEqualityResult.Create(new Distinction(typeName, expected, actual));

        if (expected is null)
        {
            return DeepEqualityResult.None();
        }

        if (_options.DifferentTypesAllowed && type != actualType)
        {
            return CompareWithTypes(expected, actual, typeName, type, actualType);
        }

        return RulesHandler
            .GetFor(type)
            .Compare(expected, actual, typeName);
    }

    internal DeepEqualityResult CompareWithTypes(object? expected, object? actual, string propertyName, Type expectedType,
        Type actualType)
    {
        if (_options.DifferentTypesAllowed && IsCompatible(expectedType, actualType, expected, actual) == false)
        {
            return CompareDifferentTypes(expected, actual, propertyName, expectedType, actualType);
        }

        var comparer = RulesHandler.GetFor(expectedType);
        return CompareValues(comparer, expected, actual, propertyName, expectedType);
    }

    private DeepEqualityResult CompareDifferentTypes(object? expected, object? actual, string propertyName, Type expectedType,
        Type actualType)
    {
        if (RulesHandler.IsIgnored(propertyName))
        {
            return DeepEqualityResult.None();
        }

        if (expected is null && actual is null)
        {
            return DeepEqualityResult.None();
        }

        if (expected is null || actual is null)
        {
            return DeepEqualityResult.Create(propertyName, expected ?? "null", actual ?? "null");
        }

        if (expectedType.IsValueType || actualType.IsValueType
            || expectedType == typeof(string) || actualType == typeof(string))
        {
            return Equals(expected, actual)
                ? DeepEqualityResult.None()
                : DeepEqualityResult.Create(new Distinction(propertyName, expected, actual));
        }

        if (IsDictionaryType(expectedType) && IsDictionaryType(actualType))
        {
            return _dictionaryCompareStrategy.CompareDifferentTypes(expected, actual, propertyName);
        }

        if (IsCollectionType(expectedType) && IsCollectionType(actualType))
        {
            return _collectionsCompareStrategy.CompareDifferentTypes(expected, actual, propertyName);
        }

        return _compareMembersStrategy.CompareDifferentTypes(expected, actual, propertyName);
    }

    private static DeepEqualityResult CompareValues(CompareValues compareValues, object? expected, object? actual,
        string propertyName, Type expectedType)
    {
        var del = CompareValuesDelegates.GetOrAdd(expectedType, CreateCompareValuesDelegate);
        return del(compareValues, expected, actual, propertyName);
    }

    private static CompareValuesDelegate CreateCompareValuesDelegate(Type expectedType)
    {
        var method = typeof(CompareValues).GetMethod(nameof(Compare))!.MakeGenericMethod(expectedType);

        var compareValuesParameter = Expression.Parameter(typeof(CompareValues), "compareValues");
        var expectedParameter = Expression.Parameter(typeof(object), "expected");
        var actualParameter = Expression.Parameter(typeof(object), "actual");
        var propertyNameParameter = Expression.Parameter(typeof(string), "propertyName");

        var call = Expression.Call(compareValuesParameter, method,
            Expression.Convert(expectedParameter, expectedType),
            Expression.Convert(actualParameter, expectedType),
            propertyNameParameter);

        return Expression.Lambda<CompareValuesDelegate>(call, compareValuesParameter, expectedParameter, actualParameter,
            propertyNameParameter).Compile();
    }

    private static bool IsDictionaryType(Type type)
    {
        return type.GetInterfaces().Prepend(type)
            .Any(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == DictionaryType);
    }

    private static bool IsCollectionType(Type type)
    {
        if (type == typeof(string))
        {
            return false;
        }

        return type.GetInterfaces().Prepend(type)
            .Any(interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == EnumerableType);
    }

    private static bool IsCompatible(Type expectedType, Type actualType, object? expected, object? actual)
    {
        if (expected is null || actual is null)
        {
            return IsNullable(expectedType);
        }

        if (expectedType.IsAssignableFrom(actualType))
        {
            return true;
        }

        var expectedUnderlying = Nullable.GetUnderlyingType(expectedType);
        return expectedUnderlying != null && expectedUnderlying == actualType;
    }

    private static bool IsNullable(Type type) =>
        type.IsValueType == false || Nullable.GetUnderlyingType(type) != null;

    private delegate DeepEqualityResult CompareValuesDelegate(CompareValues compareValues, object? expected,
        object? actual, string propertyName);
}
