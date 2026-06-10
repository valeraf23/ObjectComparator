using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ObjectsComparator.Comparator.Rules;

/// <summary>
///     Manages comparison rules and selects the appropriate comparer based on type and priority.
///     Rules are automatically sorted by priority (lower values first).
/// </summary>
internal sealed class RulesHandler
{
    private readonly Func<Type, ICompareValues> _findComparerFunc;
    private readonly Func<Type, CompareValues> _createCompareValuesFunc;
    private readonly Func<string, bool> _ignoreStrategy;

    private readonly ConcurrentDictionary<Type, ICompareValues> _ruleCache = new();
    private readonly ConcurrentDictionary<Type, CompareValues> _compareValuesCache = new();
    private readonly Rule[] _rules;
    private readonly Dictionary<string, ICustomCompareValues> _strategies;
    private readonly Dictionary<Type, ICustomCompareValues> _typeStrategies;

    public RulesHandler(List<Rule> rules, Dictionary<string, ICustomCompareValues> strategies,
        Func<string, bool> ignoreStrategy, Dictionary<Type, ICustomCompareValues>? typeStrategies = null)
    {
        _strategies = strategies;
        _ignoreStrategy = ignoreStrategy;
        _typeStrategies = typeStrategies ?? new Dictionary<Type, ICustomCompareValues>();
        // Comparator supplies rules already sorted by ascending priority.
        _rules = rules.ToArray();
        _findComparerFunc = FindComparer;
        _createCompareValuesFunc = CreateCompareValues;
    }

    public CompareValues GetFor(Type memberType)
    {
        return _compareValuesCache.GetOrAdd(memberType, _createCompareValuesFunc);
    }

    private CompareValues CreateCompareValues(Type memberType)
    {
        var comparer = _ruleCache.GetOrAdd(memberType, _findComparerFunc);
        return new CompareValues(comparer, _strategies, _ignoreStrategy, _typeStrategies);
    }

    private ICompareValues FindComparer(Type memberType)
    {
        for (var i = 0; i < _rules.Length; i++)
        {
            if (_rules[i].IsValid(memberType))
            {
                return _rules[i].Get(memberType);
            }
        }

        throw new NotSupportedException($"Not Satisfied Rule for {memberType.FullName}");
    }

    internal bool IsIgnored(string propertyName)
    {
        return _ignoreStrategy(propertyName);
    }
}