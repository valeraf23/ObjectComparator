using System;
using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Rules.Implementations
{
    internal abstract class ChainedRule : IGetRule<ICompareValues>
    {
        protected IGetProperlyRule Rule { get; set; }
        protected IGetRule<ICompareValues> Next { get; }
        protected ChainedRule(IGetRule<ICompareValues> next) => Next = next;
        public virtual ICompareValues GetFor(Type memberType) => Rule.IsValid(memberType) ? Rule.GetFor(memberType) : Next.GetFor(memberType);
    }
}