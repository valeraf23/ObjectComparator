using System;

namespace ObjectsComparator.Comparator.RepresentationDistinction
{
    public class ForDistinctionsBuilder<T>
    {
        private readonly T _actuallyValue;
        private readonly T _expectedValue;
        private readonly string _name;

        public ForDistinctionsBuilder(string name, object expectedValue, object actuallyValue)
        {
            _name = name;
            _expectedValue = (T) expectedValue;
            _actuallyValue = (T) actuallyValue;
        }

        public Distinctions WhenNot(Func<T, T, bool> func)
        {
            return func(_expectedValue, _actuallyValue)
                ? Distinctions.None()
                : Distinctions.Create(new[] {new Distinction(_name, _expectedValue, _actuallyValue)});
        }
    }
}