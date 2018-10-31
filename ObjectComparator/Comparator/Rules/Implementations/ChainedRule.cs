using System;
using ObjectComparator.Comparator.Interfaces;
using ObjectComparator.Comparator.Rules.Interfaces;

namespace ObjectComparator.Comparator.Rules.Implementations
{
    internal abstract class ChainedRule : IGetRule<ICompareValues>
    {
        protected IGetProperlyRule Rule { get; set; }
        protected IGetRule<ICompareValues> Next { get; }
        protected ChainedRule(IGetRule<ICompareValues> next) => Next = next;
        public virtual ICompareValues Get(Type memberType) => Rule.IsValid(memberType) ? Rule.Get(memberType) : Next.Get(memberType);
    }
}