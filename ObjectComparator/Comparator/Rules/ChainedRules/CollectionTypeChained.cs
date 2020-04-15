using ObjectsComparator.Comparator.Rules.Interfaces;

namespace ObjectsComparator.Comparator.Rules.ChainedRules
{
    internal class CollectionTypeChained : ReferenceTypeChained
    {
        public CollectionTypeChained(IGetProperlyRule rule, IGetProperlyRule next) : base(rule, next) { }

    }
}