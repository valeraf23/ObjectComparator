using System;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Rules;
using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections
{
    public abstract class BaseCollectionsCompareStrategy : ICollectionsCompareStrategy
    {
        private readonly Comparator _comparator;
        protected RulesHandler RulesHandler => _comparator.RulesHandler;
        protected BaseCollectionsCompareStrategy(Comparator comparator)
        {
            _comparator = comparator;
        }
      
        public abstract DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull;
        public abstract bool IsValid(Type member);
    }
}