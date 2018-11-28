using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Rules.Implementations
{
    internal class ReferenceTypeChained : ChainedRule
    {
        public ReferenceTypeChained(IGetProperlyRule rule, IGetRule<ICompareValues> next) : base(next)
        {
            Rule = rule;
        }
    }
}