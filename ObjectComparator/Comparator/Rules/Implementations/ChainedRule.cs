using System;
using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Rules.Implementations
{
    internal abstract class ChainedRule : IGetRule<ICompareValues>
    {
        protected ChainedRule(IGetRule<ICompareValues> next)
        {
            Next = next;
        }

        protected IGetProperlyRule Rule { get; set; }
        protected IGetRule<ICompareValues> Next { get; }

        public virtual ICompareValues GetFor(Type memberType)
        {
            return Rule.IsValid(memberType) ? Rule.GetFor(memberType) : Next.GetFor(memberType);
        }
    }
}