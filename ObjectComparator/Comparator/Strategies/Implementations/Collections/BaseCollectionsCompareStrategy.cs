using System;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections
{
    public abstract class BaseCollectionsCompareStrategy : ICollectionsCompareStrategy
    {
        public Comparator Comparator { get; set; }
        public abstract Distinctions Compare<T>(T expected, T actual, string propertyName);
        public abstract bool IsValid(Type member);
    }
}