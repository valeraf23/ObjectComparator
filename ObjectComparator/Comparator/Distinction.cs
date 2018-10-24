using System;
using ObjectComparator.Helpers.GuardArgument;

namespace ObjectComparator.Comparator
{
    public class Distinction : IEquatable<Distinction>
    {
        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                if (value != null)
                {
                    _name = value;
                }
            }
        }

        public object ExpectedValue { get; set; }

        public object ActuallyValue { get; set; }

        public Distinction(string name, object expectedValue, object actuallyValue)
        {
            GuardArgument.ArgumentIsNotNull(name, $"{nameof(name)} can not be null or empty");
            Name = name;
            ExpectedValue = expectedValue;
            ActuallyValue = actuallyValue;
        }

        public override string ToString() =>
            $"\nProperty name \"{_name}\":\nExpected Value :{ExpectedValue}\nActually Value :{ActuallyValue}";


        public bool Equals(Distinction other) => other != null &&
                                                 (ReferenceEquals(this, other) ||
                                                  Name.Equals(other.Name) &&
                                                  ActuallyValue.Equals(other.ActuallyValue) &&
                                                  ExpectedValue.Equals(other.ExpectedValue));

        public override bool Equals(object obj) => Equals(obj as Distinction);

        public static bool operator ==(Distinction a, Distinction b) =>
            ReferenceEquals(a, null) && ReferenceEquals(b, null) ||
            !ReferenceEquals(a, null) && a.Equals(b);

        public static bool operator !=(Distinction a, Distinction b) => !(a == b);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_name != null ? _name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExpectedValue != null ? ExpectedValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ActuallyValue != null ? ActuallyValue.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
