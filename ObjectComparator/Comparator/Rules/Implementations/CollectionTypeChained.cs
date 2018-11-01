using ObjectsComparator.Comparator.Interfaces;
using ObjectsComparator.Comparator.Rules.Interfaces;

namespace ObjectsComparator.Comparator.Rules.Implementations
{
    internal class CollectionTypeChained : ReferenceTypeChained
    {
        public CollectionTypeChained(IGetProperlyRule rule, IGetRule<ICompareValues> next) : base(rule, next) => Rule = rule;
    }
}