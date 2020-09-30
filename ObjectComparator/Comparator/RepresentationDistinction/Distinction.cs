using System;
using ObjectsComparator.Helpers.GuardArgument;

namespace ObjectsComparator.Comparator.RepresentationDistinction
{
    public class Distinction : IEquatable<Distinction>
    {
        public Distinction(string path, object? expectedValue, object? actuallyValue)
        {
            GuardArgument.ArgumentIsNotNull(path, $"{nameof(path)} can not be null or empty");
            Path = path;
            ExpectedValue = expectedValue;
            ActuallyValue = actuallyValue;
            Details = string.Empty;
        }

        public Distinction(string path, object expectedValue, object actuallyValue, string details) : this(path,
            expectedValue, actuallyValue) =>
            Details = details;

        public string Path { get; }
        public string Details { get; }
        public object? ExpectedValue { get; }
        public object? ActuallyValue { get; }

        public bool Equals(Distinction? other) =>
            other != null &&
            (ReferenceEquals(this, other) ||
             Path.Equals(other.Path) && Details.Equals(other.Details));

        public override string ToString()
        {
            var info = $"Path: \"{Path}\":\nExpected Value :{ExpectedValue}\nActually Value :{ActuallyValue}";
            return string.IsNullOrEmpty(Details) ? info : $"{info}\n{nameof(Details)} : {Details}";
        }

        public override bool Equals(object? obj) => Equals(obj as Distinction);

        public override int GetHashCode() => HashCode.Combine(Path, Details);

        public static bool operator == (Distinction? a, Distinction? b) =>
            a is null && b is null ||
            a?.Equals(b) == true;

        public static bool operator != (Distinction a, Distinction b) =>
            !(a == b);
    }
}