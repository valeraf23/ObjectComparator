using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;
using System;
using System.Collections.Generic;

namespace ObjectsComparator.Comparator.Rules;

public class CompareValues
{
    private readonly ICompareValues _defaultComparer;
    private readonly Func<string, bool> _shouldIgnore;
    private readonly Dictionary<string, ICustomCompareValues> _strategies;
    private readonly Dictionary<Type, ICustomCompareValues> _typStrategies;

    public CompareValues(
        ICompareValues defaultComparer,
        Dictionary<string, ICustomCompareValues> strategies,
        Func<string, bool> shouldIgnore,
        Dictionary<Type, ICustomCompareValues> typStrategies)
    {
        _defaultComparer = defaultComparer;
        _strategies = strategies;
        _shouldIgnore = shouldIgnore;
        _typStrategies = typStrategies;
    }

    public DeepEqualityResult Compare<T>(T expected, T actual, string propertyPath)
    {
        if (string.IsNullOrEmpty(propertyPath))
        {
            throw new ArgumentException("Property path cannot be null or empty.", nameof(propertyPath));
        }

        if (_shouldIgnore(propertyPath))
        {
            return DeepEqualityResult.None();
        }

        if (TryGetStrategy(propertyPath, out var strategy))
        {
            return strategy!.Compare(expected, actual, propertyPath);
        }

        if (_typStrategies.TryGetValue(typeof(T), out var typeStrategy))
        {
            return typeStrategy.Compare(expected, actual, propertyPath);
        }

        return CompareOrDefault(expected, actual, propertyPath);

    }

    private bool TryGetStrategy(string propertyPath, out ICustomCompareValues? strategy)
    {
        strategy = null;

        if (_strategies.IsEmpty())
        {
            return false;
        }

        if (_strategies.TryGetValue(propertyPath, out strategy))
        {
            return true;
        }

        if (!PropertyPathNormalizer.ContainsIndexer(propertyPath))
        {
            return false;
        }

        var normalizedPath = PropertyPathNormalizer.Normalize(propertyPath);
        return _strategies.TryGetValue(normalizedPath, out strategy);
    }

    private DeepEqualityResult CompareOrDefault<T>(T expected, T actual, string propertyPath)
    {
        if (expected is null && actual is not null)
        {
            return DeepEqualityResult.Create(propertyPath, "null", actual);
        }

        if (expected is not null && actual is null)
        {
            return DeepEqualityResult.Create(propertyPath, expected, "null");
        }

        return expected is null
            ? DeepEqualityResult.None()
            : _defaultComparer.Compare(expected, actual, propertyPath);
    }
}