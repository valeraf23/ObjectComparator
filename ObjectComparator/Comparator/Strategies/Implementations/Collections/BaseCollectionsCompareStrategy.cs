using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Rules;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using System;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections;

public abstract class BaseCollectionsCompareStrategy : ICollectionsCompareStrategy
{
    protected BaseCollectionsCompareStrategy(Comparator comparator)
    {
        Comparator = comparator;
    }

    protected Comparator Comparator { get; }

    protected RulesHandler RulesHandler => Comparator.RulesHandler;

    public abstract DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull;
    public abstract bool IsValid(Type member);
}