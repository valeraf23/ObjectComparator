using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ObjectsComparator.Comparator.Rules;

/// <summary>
/// Manages comparison rules and selects the appropriate comparer based on type and priority.
/// Rules are automatically sorted by priority (lower values first).
/// </summary>
internal sealed class RulesHandler
{
    private readonly Func<string, bool> _ignoreStrategy;
    private readonly Rule[] _rules;
    private readonly Dictionary<string, ICustomCompareValues> _strategies;
    private readonly Dictionary<Type, ICustomCompareValues> _typeStrategies;
    
    private readonly ConcurrentDictionary<Type, ICompareValues> _ruleCache = new();

    public RulesHandler(List<Rule> rules, Dictionary<string, ICustomCompareValues> strategies,
        Func<string, bool> ignoreStrategy, Dictionary<Type, ICustomCompareValues>? typeStrategies = null)
    {
        _strategies = strategies;
        _ignoreStrategy = ignoreStrategy;
        _typeStrategies = typeStrategies ?? new Dictionary<Type, ICustomCompareValues>();
        _rules = rules.OrderBy(r => r.Priority).ToArray();
    }

    public CompareValues GetFor(Type memberType)
    {
        var comparer = _ruleCache.GetOrAdd(memberType, type =>
        {
            var rule = _rules.FirstOrDefault(r => r.IsValid(type))?.Get(type);
            return rule ?? throw new NotSupportedException($"Not Satisfied Rule for {type.FullName}");
        });

        return new CompareValues(comparer, _strategies, _ignoreStrategy, _typeStrategies);
    }

    internal bool IsIgnored(string propertyName)
    {
        return _ignoreStrategy(propertyName);
    }
}