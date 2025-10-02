using ObjectsComparator.Comparator.Helpers;
using System;
using System.Collections.Generic;
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

    public Comparator(Dictionary<string, ICustomCompareValues> customStrategies, Func<string, bool> ignoreStrategy, ComparatorOptions options)
    {
        var rules = new List<Rule>(6)
        {
            Rule.CreateFor(new ComparePrimitiveTypesStrategy())
        };

        if (options.IsSkipped(StrategyType.Equality) == false)
        {
            rules.Add(Rule.CreateFor(new EqualityStrategy()));
        }
        if (options.IsSkipped(StrategyType.OverridesEquals) == false)
        {
            rules.Add(Rule.CreateFor(new OverridesEqualsStrategy()));
        }
        if (options.IsSkipped(StrategyType.CompareTo) == false)
        {
            rules.Add(Rule.CreateFor(new ComparablesStrategy()));
        }

        rules.Add(Rule.CreateFor<ICollectionsCompareStrategy>(new CollectionsCompareStrategy(this),
            new DictionaryCompareStrategy(this)));

        rules.Add(Rule.CreateFor(new CompareMembersStrategy(this)));

        RulesHandler = new RulesHandler(rules, customStrategies, ignoreStrategy);
    }

    public DeepEqualityResult Compare<T>(T expected, T actual)
    {
        var type = expected?.GetType() ?? typeof(T);
        var actualType = actual?.GetType() ?? typeof(T);
        var typeName = type.ToFriendlyTypeName();
        if (type != actualType)
        {
            type = typeof(T);
            typeName = type.ToFriendlyTypeName();
        }

        if (expected != null && actual == null || expected == null && actual != null)
            return DeepEqualityResult.Create(new Distinction(typeName, expected, actual));

        return RulesHandler
            .GetFor(type)
            .Compare(expected, actual, typeName);
    }
}