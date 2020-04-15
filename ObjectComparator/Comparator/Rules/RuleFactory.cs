using ObjectsComparator.Comparator.Rules.ChainedRules;
using ObjectsComparator.Comparator.Rules.Implementations;
using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Rules
{
    public static class RuleFactory
    {
        public static IGetRule<ICompareValues> Create(
            IGetProperlyRule ruleForCollectionTypes,
            IGetProperlyRule ruleForReferenceTypes,
            IGetProperlyRule ruleForValuesTypes
        )
        {
            return new CollectionTypeChained(ruleForCollectionTypes,
                new ReferenceTypeChained(ruleForReferenceTypes,
                    new StructTypeChained(ruleForValuesTypes,
                        new NotSatisfiedRule())));
        }
    }
}