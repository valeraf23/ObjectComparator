using System;
using System.Collections.Generic;
using ObjectComparator.Comparator.Interfaces;

namespace ObjectComparator.Comparator.Implementations
{
    public sealed class CompareObjectsStrategy : ICompareObjectStrategy
    {
        public DistinctionsCollection Compare<T>(T valueA, T valueB, string propertyName) =>
            ComparatorExtension.BaseGetDifferenceBetweenObjects(valueA, (object) valueB, Strategies, propertyName,
                Ignore);

        public bool IsValid(Type member) => member.IsClass && member != typeof(string);
        public IList<string> Ignore { get; set; } = new List<string>();
        public IDictionary<string, ICompareValues> Strategies { get; set; } = new Dictionary<string, ICompareValues>();
    }
}