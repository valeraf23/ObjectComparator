using ObjectComparator.Comparator.Interfaces;
using ObjectComparator.Comparator.Rules.Implementations;
using ObjectComparator.Comparator.Rules.Interfaces;

namespace ObjectComparator.Comparator.Rules
{
    public static class RuleFactory
    {
        public static IGetRule<ICompareValues> Create(
            IGetProperlyRule ruleForCollectionTypes,
            IGetProperlyRule ruleForReferenceTypes,
            IGetProperlyRule ruleForValuesTypes
        ) =>
            new CollectionTypeChained(ruleForCollectionTypes,
                new ReferenceTypeChained(ruleForReferenceTypes,
                    new StructTypeChained(ruleForValuesTypes,
                        new NotSatisfiedRule())));
    }
}