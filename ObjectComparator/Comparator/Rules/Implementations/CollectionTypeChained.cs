using ObjectComparator.Comparator.Interfaces;
using ObjectComparator.Comparator.Rules.Interfaces;

namespace ObjectComparator.Comparator.Rules.Implementations
{
    internal class CollectionTypeChained : ReferenceTypeChained
    {
        public CollectionTypeChained(IGetProperlyRule rule, IGetRule<ICompareValues> next) : base(rule, next) => Rule = rule;
    }
}