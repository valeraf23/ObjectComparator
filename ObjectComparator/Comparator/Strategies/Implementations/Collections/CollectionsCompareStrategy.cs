using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Helpers.Extensions;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections
{
    public class CollectionsCompareStrategy : BaseCollectionsCompareStrategy
    {
        public override Distinctions Compare<T>(T expected, T actual, string propertyName)
        {
            var diff = Distinctions.Create();
            var listA = (dynamic) expected;
            var listB = (dynamic) actual;

            if (typeof(T).ImplementsGenericInterface(typeof(IList<>)))
            {
                var lengthA = Enumerable.Count(listA);
                var lengthB = Enumerable.Count(listB);
                if (lengthA != lengthB)
                    DistinctionsForCollectionsWithDifferentLength(propertyName, lengthA, lengthB);
                for (var i = 0; i < lengthA; i++)
                    diff.AddRange(Comparator.GetDistinctions($"{propertyName}[{i}]", listA[i], listB[i]));

                return diff;
            }

            ForOtherCollection(listA, listB, propertyName, diff);
            return diff;
        }

        private void ForOtherCollection(dynamic a, dynamic b, string propertyName, Distinctions collectDistinctions)
        {
            var first = a.GetEnumerator();
            var second = b.GetEnumerator();

            var iteration = 0;
            do
            {
                bool isNextA = first.MoveNext();
                bool isNextB = second.MoveNext();
                if (isNextA != isNextB)
                {
                    collectDistinctions.AddRange(DistinctionsForCollectionsWithDifferentLength(propertyName, a, b));
                    return;
                }

                if (!isNextA) return;

                var value1 = first.Current;
                var value2 = second.Current;
                collectDistinctions.AddRange(Comparator.GetDistinctions($"{propertyName}[{iteration}]", value1,
                    value2));
                iteration++;
            } while (true);
        }

        private static Distinctions DistinctionsForCollectionsWithDifferentLength(string propertyName, int first,
            int second)
        {
            return Distinctions.Create(new Distinction(
                $"Property \"{propertyName}\": Collection has different length", $"{second}",
                $"{first}"));
        }

        public override bool IsValid(Type member)
        {
            return member.GetInterfaces().Contains(typeof(IEnumerable)) && member != typeof(string);
        }
    }
}