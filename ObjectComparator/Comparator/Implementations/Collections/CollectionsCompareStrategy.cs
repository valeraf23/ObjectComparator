using System;
using System.Collections;
using System.Linq;

namespace ObjectsComparator.Comparator.Implementations.Collections
{
    public class CollectionsCompareStrategy : BaseCollectionsCompareStrategy
    {
        public override DistinctionsCollection Compare<T>(T valueA, T valueB, string propertyName)
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
                (dc, i) => dc.AddRange(
                    Comparator.GetDifference(listA[i], listB[i], $"{propertyName}[{i}]")));
        }

        public override bool IsValid(Type member) =>
            member.GetInterfaces().Contains(typeof(IEnumerable)) && member != typeof(string);
    }
}