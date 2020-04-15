using ObjectsComparator.Comparator.Rules.Interfaces;

namespace ObjectsComparator.Comparator.Rules.ChainedRules
{
    internal class StructTypeChained : ChainedRule
    {
        public StructTypeChained(IGetProperlyRule rule, IGetProperlyRule next) : base(rule, next)
        {
        }
    }
}