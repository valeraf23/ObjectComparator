using System;
using ObjectsComparator.Helpers.GuardArgument;

namespace ObjectsComparator.Comparator.RepresentationDistinction
{
    public class Distinction : IEquatable<Distinction>
    {
        private string _name;

        public Distinction(string name, object expectedValue, object actuallyValue)
        {
            GuardArgument.ArgumentIsNotNull(name, $"{nameof(name)} can not be null or empty");
            Name = name;
            ExpectedValue = expectedValue;
            ActuallyValue = actuallyValue;
        }

        public string Name
        {
            get => _name;
            set
            {
                if (value != null) _name = value;
            }
        }

        public object ExpectedValue { get; set; }

        public object ActuallyValue { get; set; }


        public bool Equals(Distinction other) =>
            other != null &&
            (ReferenceEquals(this, other) ||
             Name.Equals(other.Name) &&
             ActuallyValue.Equals(other.ActuallyValue) &&
             ExpectedValue.Equals(other.ExpectedValue));

        public override string ToString() =>
            $"\nProperty name \"{_name}\":\nExpected Value :{ExpectedValue}\nActually Value :{ActuallyValue}";

        public override bool Equals(object obj)
        {
            return Equals(obj as Distinction);
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

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _name != null ? _name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (ExpectedValue != null ? ExpectedValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ActuallyValue != null ? ActuallyValue.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}