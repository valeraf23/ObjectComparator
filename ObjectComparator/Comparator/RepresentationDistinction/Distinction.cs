using System;
using ObjectsComparator.Helpers.GuardArgument;

namespace ObjectsComparator.Comparator.RepresentationDistinction
{
    public class Distinction : IEquatable<Distinction>
    {
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

        public bool Equals(Distinction other)
        {
            return other != null &&
                   (ReferenceEquals(this, other) ||
                    Name.Equals(other.Name) && Details.Equals(other.Details));
        }

        public override string ToString()
        {
            var info = $"\nProperty name \"{Name}\":\nExpected Value :{ExpectedValue}\nActually Value :{ActuallyValue}";
            return string.IsNullOrEmpty(Details) ? info : $"{info}\n{nameof(Details)} : {Details}";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Distinction);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Details);
        }

        public static bool operator ==(Distinction a, Distinction b)
        {
            return a is null && b is null ||
                   a?.Equals(b) == true;
        }

        public static bool operator !=(Distinction a, Distinction b)
        {
            return !(a == b);
        }
    }
}