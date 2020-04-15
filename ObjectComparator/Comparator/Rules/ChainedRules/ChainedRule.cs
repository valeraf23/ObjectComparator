using System;
using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Rules.ChainedRules
{
    internal abstract class ChainedRule : IGetProperlyRule
    {
        protected ChainedRule(IGetProperlyRule rule, IGetProperlyRule next)
        {
            Rule = rule;
            Next = next;
        }

        protected IGetProperlyRule Rule { get; }
        protected IGetProperlyRule Next { get; }

        public virtual ICompareValues GetFor(Type memberType) =>
            IsValid(memberType) ? Rule.GetFor(memberType) : Next.GetFor(memberType);

        public bool IsValid(Type member) => Rule.IsValid(member);
    }
}