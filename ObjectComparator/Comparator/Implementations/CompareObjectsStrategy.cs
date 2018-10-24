using System;
using System.Collections.Generic;
using ObjectComparator.Comparator.Interfaces;
using ObjectComparator.Comparator.StrategiesForCertainProperties;

namespace ObjectComparator.Comparator.Implementations
{
    public sealed class CompareObjectsStrategy : ICompareObjectStrategy
    {
        public DistinctionsCollection Compare<T>(T valueA, T valueB, string propertyName) =>
            ComparatorExtension.BaseGetDifferenceBetweenObjects(valueA, (object) valueB, Strategies, propertyName,
                Ignore);

        public bool IsValid(Type member) => member.IsClass && member != typeof(string);
        public IList<string> Ignore { get; set; } = new List<string>();
        public IList<IMemberStrategy> Strategies { get; set; } = new List<IMemberStrategy>();
    }
}