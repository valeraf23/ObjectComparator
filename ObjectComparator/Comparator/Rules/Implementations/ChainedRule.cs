using ObjectComparator.Comparator.Interfaces;
using ObjectComparator.Comparator.Rules.Interfaces;

namespace ObjectComparator.Comparator.Rules.Implementations
{
    internal abstract class ChainedRule : IGetRule<ICompareValues>
    {
        protected IGetProperlyRule Rule { get; set; }
        protected IGetRule<ICompareValues> Next { get; }
        protected ChainedRule(IGetRule<ICompareValues> next) => Next = next;
        public virtual ICompareValues Get<T>() => Rule.IsValid(typeof(T)) ? Rule.Get<T>() : Next.Get<T>();
    }
}