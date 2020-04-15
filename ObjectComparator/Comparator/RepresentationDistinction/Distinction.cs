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
        }

        public string Name { get; }

        public object ExpectedValue { get; }

        public object ActuallyValue { get; }

        public bool Equals(Distinction other) =>
            other != null &&
            (ReferenceEquals(this, other) ||
             Name.Equals(other.Name) &&
             ActuallyValue.Equals(other.ActuallyValue) &&
             ExpectedValue.Equals(other.ExpectedValue));

        public override string ToString() =>
            $"\nProperty name \"{Name}\":\nExpected Value :{ExpectedValue}\nActually Value :{ActuallyValue}";

        public override bool Equals(object obj) => Equals(obj as Distinction);

        public static bool operator ==(Distinction a, Distinction b)
        {
            return a is null && b is null ||
                   a?.Equals(b) == true;
        }

        public static bool operator !=(Distinction a, Distinction b)
        {
            return !(a == b);
        }

        public override int GetHashCode() => HashCode.Combine(Name, ExpectedValue, ActuallyValue);
    }
}