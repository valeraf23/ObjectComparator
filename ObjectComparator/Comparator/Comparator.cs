using System;
using System.Collections.Generic;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Rules;
using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Implementations;
using ObjectsComparator.Comparator.Strategies.Implementations.Collections;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;

namespace ObjectsComparator.Comparator;

public sealed class Comparator
{
    public readonly RulesHandler RulesHandler;

    public Comparator(Dictionary<string, ICustomCompareValues> customStrategies, Func<string, bool> ignoreStrategy)
    {
        RulesHandler = new RulesHandler(new[]
            {
                Rule.CreateFor(new ComparePrimitiveTypesStrategy()),
                Rule.CreateFor(new EqualityStrategy()),
                Rule.CreateFor(new OverridesEqualsStrategy()),
                Rule.CreateFor(new ComparablesStrategy()),
                Rule.CreateFor<ICollectionsCompareStrategy>(new CollectionsCompareStrategy(this),
                    new DictionaryCompareStrategy(this)),
                Rule.CreateFor(new CompareMembersStrategy(this))
            },
            customStrategies,
            ignoreStrategy);
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