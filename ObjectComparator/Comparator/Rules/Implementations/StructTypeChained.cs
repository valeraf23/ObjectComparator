using ObjectsComparator.Comparator.Interfaces;
using ObjectsComparator.Comparator.Rules.Interfaces;

namespace ObjectsComparator.Comparator.Rules.Implementations
{
    internal class StructTypeChained : ChainedRule
    {
        public StructTypeChained(IGetProperlyRule rule, IGetRule<ICompareValues> next) : base(next) => Rule = rule;
    }
}