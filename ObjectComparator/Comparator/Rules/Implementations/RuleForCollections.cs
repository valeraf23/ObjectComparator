using System.Collections.Generic;
using ObjectsComparator.Comparator.Strategies.Implementations.Collections;
using ObjectsComparator.Helpers.Extensions;

namespace ObjectsComparator.Comparator.Rules.Implementations
{
    public class RuleForCollections : Rule<BaseCollectionsCompareStrategy>
    {
        public RuleForCollections(Comparator comparator, IList<BaseCollectionsCompareStrategy> others) :
            base(new CollectionsCompareStrategy(), others)
        {
            Strategies.ForEach(s => s.Comparator = comparator);
        }
    }
}