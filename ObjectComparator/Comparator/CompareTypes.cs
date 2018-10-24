using System.Collections.Generic;
using ObjectComparator.Comparator.Implementations;
using ObjectComparator.Comparator.Interfaces;
using ObjectComparator.Comparator.Rules;
using ObjectComparator.Comparator.Rules.Implementations;
using ObjectComparator.Comparator.StrategiesForCertainProperties;
using ObjectComparator.Helpers.Extensions;

namespace ObjectComparator.Comparator
{
    public sealed class CompareTypes
    {
        public CompareTypes()
        {
            RuleForValuesTypes = new Rule<ICompareStructStrategy>(new CompareValueTypesStrategy());
            RuleForCollectionTypes = new Rule<ICollectionsCompareStrategy>(new CollectionsCompareStrategy(this));
            RuleForReferenceTypes = new Rule<ICompareObjectStrategy>(new CompareObjectsStrategy());
        }

        public Rule<ICompareObjectStrategy> RuleForReferenceTypes { get; }
        public Rule<ICompareStructStrategy> RuleForValuesTypes { get; }
        public Rule<ICollectionsCompareStrategy> RuleForCollectionTypes { get; }

        public DistinctionsCollection Compare<T>(T valueA, T valueB, string propertyName) => RuleFactory.Create(RuleForCollectionTypes, RuleForReferenceTypes, RuleForValuesTypes).Get<T>()
            .Compare(valueA, valueB, propertyName);

        public void SetIgnore(IList<string> ignore)
        {
            if (ignore.IsEmpty()) return;
            RuleForReferenceTypes.Default.Ignore = ignore;
            RuleForReferenceTypes.Others.ForEach(x => x.Ignore = ignore);
        }
        public void SetStrategies(IList<IMemberStrategy> strategies)
        {
            if (strategies.IsEmpty()) return;
            RuleForReferenceTypes.Default.Strategies = strategies;
            RuleForReferenceTypes.Others.ForEach(x => x.Strategies = strategies);
        }
    }
}