using System;
using ObjectsComparator.Comparator.Interfaces;
using ObjectsComparator.Comparator.Rules.Interfaces;

namespace ObjectsComparator.Comparator.Rules.Implementations
{
    internal abstract class ChainedRule : IGetRule<ICompareValues>
    {
        protected IGetProperlyRule Rule { get; set; }
        protected IGetRule<ICompareValues> Next { get; }
        protected ChainedRule(IGetRule<ICompareValues> next) => Next = next;
        public virtual ICompareValues Get(Type memberType) => Rule.IsValid(memberType) ? Rule.Get(memberType) : Next.Get(memberType);
    }
}