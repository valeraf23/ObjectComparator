using System;

namespace ObjectsComparator.Comparator
{
    public class ForDistinctionsCollectionBuilder<T>
    {
        private readonly string _name;
        private readonly T _expectedValue;
        private readonly T _actuallyValue;

        public ForDistinctionsCollectionBuilder(string name, object expectedValue, object actuallyValue)
        {
            _name = name;
            _expectedValue = (T)expectedValue;
            _actuallyValue = (T)actuallyValue;
        }

        public DistinctionsCollection WhenNot(Func<T, T, bool> func) => func(_expectedValue, _actuallyValue)
            ? new DistinctionsCollection()
            : new DistinctionsCollection(new[] {new Distinction(_name, _expectedValue, _actuallyValue)});
    }
}