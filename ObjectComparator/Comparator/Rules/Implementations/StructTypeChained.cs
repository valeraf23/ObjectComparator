using ObjectComparator.Comparator.Interfaces;
using ObjectComparator.Comparator.Rules.Interfaces;

namespace ObjectComparator.Comparator.Rules.Implementations
{
    internal class StructTypeChained : ChainedRule
    {
        public StructTypeChained(IGetProperlyRule rule, IGetRule<ICompareValues> next) : base(next) => Rule = rule;
    }
}