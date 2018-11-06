using System;
using ObjectsComparator.Comparator.Interfaces;

namespace ObjectsComparator.Comparator.Implementations.Collections
{
    public abstract class BaseCollectionsCompareStrategy : ICollectionsCompareStrategy
    {
        public Comparator Comparator { get; set; }
        public abstract DistinctionsCollection Compare<T>(T valueA, T valueB, string propertyName);
        public abstract bool IsValid(Type member);
    }
}