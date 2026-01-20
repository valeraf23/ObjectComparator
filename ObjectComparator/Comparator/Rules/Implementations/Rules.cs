using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.GuardArgument;
using System;
using System.Linq;

namespace ObjectsComparator.Comparator.Rules.Implementations;

/// <summary>
/// A rule that wraps multiple strategies, selecting the first valid one.
/// </summary>
internal sealed class Rules<T> : Rule<T> where T : class, IStrategy
{
    public Rules(T defaultRule, RulePriority priority, params T[] others) : base(defaultRule, priority)
    {
        GuardArgument.ArgumentIsNotNull(others);
        Strategies = others;
    }

    public T[] Strategies { get; }

    public override ICompareValues Get(Type memberType)
    {
        return Strategies.FirstOrDefault(x => x.IsValid(memberType)) ?? Default;
    }
}