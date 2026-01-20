using ObjectsComparator.Comparator.Rules.Implementations;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using System;

namespace ObjectsComparator.Comparator.Rules.Interfaces;

/// <summary>
/// Abstract base class for comparison rules with explicit priority ordering.
/// </summary>
public abstract class Rule : IGetRule<ICompareValues>, IValidator
{
    public abstract RulePriority Priority { get; }

    public abstract ICompareValues Get(Type memberType);

    public abstract bool IsValid(Type member);

    public static Rule CreateFor<T>(T strategy, RulePriority priority) where T : class, IStrategy
    {
        return new Rule<T>(strategy, priority);
    }

    public static Rule CreateFor<T>(T defaultRule, RulePriority priority, params T[] others) where T : class, IStrategy
    {
        return new Rules<T>(defaultRule, priority, others);
    }

    public static Rule CreateFor<T>(T strategy) where T : class, IStrategy
    {
        return new Rule<T>(strategy, RulePriority.Members);
    }

    public static Rule CreateFor<T>(T defaultRule, params T[] others) where T : class, IStrategy
    {
        return new Rules<T>(defaultRule, RulePriority.Members, others);
    }
}