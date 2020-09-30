using System;

namespace ObjectsComparator.Comparator.RepresentationDistinction
{
    public class ForDistinctionsBuilder<T> where T : notnull
    {
        private readonly T _actuallyValue;
        private readonly T _expectedValue;
        private readonly string _name;
        private readonly string _details;

        public ForDistinctionsBuilder(string name, T expectedValue, T actuallyValue)
        {
            _name = name;
            _expectedValue = expectedValue;
            _actuallyValue = actuallyValue;
            _details = string.Empty;
        }

        public ForDistinctionsBuilder(string name, T expectedValue, T actuallyValue, string details) : this(name,
            expectedValue, actuallyValue) => _details = details;

        public DeepEqualityResult WhenNot(Func<T, T, bool> func) =>
            func(_expectedValue, _actuallyValue)
                ? DeepEqualityResult.None()
                : DeepEqualityResult.Create(new Distinction(_name, _expectedValue, _actuallyValue, _details));
    }
}