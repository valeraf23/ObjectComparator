using System;
using System.Collections;
using System.Linq;
using ObjectsComparator.Comparator.RepresentationDistinction;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections
{
    public class CollectionsCompareStrategy : BaseCollectionsCompareStrategy
    {
        public override Distinctions Compare<T>(T expected, T actual, string propertyName)
        {
            var listA = ((IEnumerable) expected).Cast<dynamic>().ToList();
            var listB = ((IEnumerable) actual).Cast<dynamic>().ToList();

            if (listA.Count != listB.Count)
                return Distinctions.Create(new Distinction(
                    $"Property \"{propertyName}\": Collection has different length",
                    $"{listA.Count}",
                    $"{listB.Count}"));

            return Enumerable.Range(0, listA.Count).Aggregate(Distinctions.Create(),
                (dc, i) => dc.AddRange(
                    Comparator.GetDistinctions($"{propertyName}[{i}]", listA[i], listB[i])));
        }

        public override bool IsValid(Type member) =>
            member.GetInterfaces().Contains(typeof(IEnumerable)) && member != typeof(string);
    }
}