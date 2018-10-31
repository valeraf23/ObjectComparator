using System;
using System.Collections;
using System.Linq;
using ObjectComparator.Comparator.Interfaces;

namespace ObjectComparator.Comparator.Implementations
{
    public sealed class CollectionsCompareStrategy : ICollectionsCompareStrategy
    {
        private readonly CompareObjectsStrategy _compareObjectsStrategy;
        public CollectionsCompareStrategy(CompareObjectsStrategy compareObjectsStrategy) => _compareObjectsStrategy = compareObjectsStrategy;

        public DistinctionsCollection Compare<T>(T valueA, T valueB, string propertyName)
        {
            var listA = ((IEnumerable) valueA).Cast<dynamic>().ToList();
            var listB = ((IEnumerable) valueB).Cast<dynamic>().ToList();

            if (listA.Count != listB.Count)
            {
                return new DistinctionsCollection
                {
                    new Distinction("Collection has different length", $"{listA.Count}",
                        $"{listB.Count}")
                };
            }

            return Enumerable.Range(0, listA.Count).Aggregate(new DistinctionsCollection(),
                (dc, i) => dc.AddRange(_compareObjectsStrategy.GetDifference(listA[i], listB[i], $"{propertyName}[{i}]")));
        }

        public bool IsValid(Type member) =>
            member.GetInterfaces().Contains(typeof(IEnumerable)) && member != typeof(string);
    }
}