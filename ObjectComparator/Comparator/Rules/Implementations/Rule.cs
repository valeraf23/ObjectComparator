using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.GuardArgument;
using System;

namespace ObjectsComparator.Comparator.Rules.Implementations;

/// <summary>
///     A rule that wraps a single strategy with an explicit priority.
/// </summary>
internal class Rule<T> : Rule where T : class, IStrategy
{
    private readonly RulePriority _priority;

    public Rule(T defaultRule, RulePriority priority = RulePriority.Members)
    {
        GuardArgument.ArgumentIsNotNull(defaultRule);
        Default = defaultRule;
        _priority = priority;
    }

    protected T Default { get; }

    public override RulePriority Priority => _priority;

    public override ICompareValues Get(Type memberType)
    {
        return Default;
    }

    public override bool IsValid(Type member)
    {
        return Default.IsValid(member);
    }
}