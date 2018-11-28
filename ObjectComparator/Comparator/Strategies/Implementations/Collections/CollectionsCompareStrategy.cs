using System;
using System.Collections;
using System.Linq;
using ObjectsComparator.Comparator.RepresentationDistinction;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections
{
    public class CollectionsCompareStrategy : BaseCollectionsCompareStrategy
    {
        public override Distinctions Compare<T>(T valueA, T valueB, string propertyName)
        {
            var listA = ((IEnumerable) valueA).Cast<dynamic>().ToList();
            var listB = ((IEnumerable) valueB).Cast<dynamic>().ToList();

            if (listA.Count != listB.Count)
                return new Distinctions
                {
                    new Distinction("Collection has different length", $"{listA.Count}",
                        $"{listB.Count}")
                };

            return Enumerable.Range(0, listA.Count).Aggregate(new Distinctions(),
                (dc, i) => dc.AddRange(
                    Comparator.GetDifference(listA[i], listB[i], $"{propertyName}[{i}]")));
        }

        public override bool IsValid(Type member)
        {
            return member.GetInterfaces().Contains(typeof(IEnumerable)) && member != typeof(string);
        }
    }
}