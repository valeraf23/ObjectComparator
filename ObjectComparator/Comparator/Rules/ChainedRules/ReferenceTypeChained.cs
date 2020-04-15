using ObjectsComparator.Comparator.Rules.Interfaces;

namespace ObjectsComparator.Comparator.Rules.ChainedRules
{
    internal class ReferenceTypeChained : ChainedRule
    {
        public ReferenceTypeChained(IGetProperlyRule rule, IGetProperlyRule next) : base(rule, next)
        {
        }
    }
}