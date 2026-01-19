using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectsComparator.Comparator.Rules;

public class RulesHandler
{
    private readonly Func<string, bool> _ignoreStrategy;
    private readonly List<Rule> _rules;
    private readonly Dictionary<string, ICustomCompareValues> _strategies;
    private readonly Dictionary<Type, ICustomCompareValues> _typeStrategies;

    public RulesHandler(List<Rule> rules, Dictionary<string, ICustomCompareValues> strategies,
        Func<string, bool> ignoreStrategy, Dictionary<Type, ICustomCompareValues>? typeStrategies = null)
    {
        _strategies = strategies;
        _ignoreStrategy = ignoreStrategy;
        _typeStrategies = typeStrategies ?? new Dictionary<Type, ICustomCompareValues>();
        _rules = rules;
    }

    public CompareValues GetFor(Type memberType)
    {
        var rule = _rules.FirstOrDefault(r => r.IsValid(memberType))?.Get(memberType) ??
                   throw new NotSupportedException($"Not Satisfied Rule for {memberType.FullName}");

        return new CompareValues(rule, _strategies, _ignoreStrategy, _typeStrategies);
    }

    internal bool IsIgnored(string propertyName)
    {
        return _ignoreStrategy(propertyName);
    }
}