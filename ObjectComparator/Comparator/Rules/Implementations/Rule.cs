using System;
using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.GuardArgument;

namespace ObjectsComparator.Comparator.Rules.Implementations
{
    public class Rule<T> : Rule where T : class, IStrategy
    {
        public Rule(T defaultRule)
        {
            GuardArgument.ArgumentIsNotNull(defaultRule);
            Default = defaultRule;
        }

        protected T Default { get; }

        public override ICompareValues Get(Type memberType) => Default;

        public override bool IsValid(Type member) => Default.IsValid(member);
    }
}