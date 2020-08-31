using System;
using ObjectsComparator.Helpers.GuardArgument;

namespace ObjectsComparator.Comparator.RepresentationDistinction
{
    public readonly struct Distinction : IEquatable<Distinction>
    {
        public override bool Equals(object obj)
        {
            return obj is Distinction other && Equals(other);
        }

        public Distinction(string name, object expectedValue, object actuallyValue)
        {
            GuardArgument.ArgumentIsNotNull(name, $"{nameof(name)} can not be null or empty");
            Name = name;
            ExpectedValue = expectedValue;
            ActuallyValue = actuallyValue;
            Details = string.Empty;
        }

        public Distinction(string name, object expectedValue, object actuallyValue, string details) : this(name,
            expectedValue, actuallyValue)
        {
            Details = details;
        }

        public string Name { get; }
        public string Details { get; }

        public object ExpectedValue { get; }

        public object ActuallyValue { get; }

        public bool Equals(Distinction other) =>
            Name.Equals(other.Name);

        public override string ToString()
        {
            var info = $"\nProperty name \"{Name}\":\nExpected Value :{ExpectedValue}\nActually Value :{ActuallyValue}";
            return string.IsNullOrEmpty(Details) ? info : $"{info}\n{nameof(Details)} : {Details}";
        }
        
        public static bool operator ==(Distinction a, Distinction b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Distinction a, Distinction b)
        {
            return !(a == b);
        }

        public override int GetHashCode() => HashCode.Combine(Name);
    }
}