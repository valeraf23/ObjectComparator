using ObjectsComparator.Helpers.GuardArgument;
using System;

namespace ObjectsComparator.Comparator.RepresentationDistinction;

public class Distinction : IEquatable<Distinction>
{
    public Distinction(string path, object? expectedValue, object? actualValue)
    {
        GuardArgument.ArgumentIsNotNull(path, $"{nameof(path)} cannot be null or empty");
            
        Path = path;
        ExpectedValue = expectedValue;
        ActualValue = actualValue;
        Details = string.Empty;
    }

    public Distinction(string path, object expectedValue, object actualValue, string details) 
        : this(path, expectedValue, actualValue) => 
        Details = details;

    public string Path { get; }
    public string Details { get; }
    public object? ExpectedValue { get; }
    public object? ActualValue { get; }

    public override bool Equals(object? obj) => Equals(obj as Distinction);

    public bool Equals(Distinction? other) =>
        other != null && 
        (ReferenceEquals(this, other) || StructuralEquality(other));

    private bool StructuralEquality(Distinction other)
    {
        return Path == other.Path
               && Details == other.Details
               && Equals(ExpectedValue, other.ExpectedValue)
               && Equals(ActualValue, other.ActualValue);
    }

    public override int GetHashCode() => HashCode.Combine(Path, Details, ExpectedValue, ActualValue);

    public override string ToString()
    {
        var info = $"Path: \"{Path}\":\nExpected Value: {ExpectedValue}\nActual Value: {ActualValue}";
        return string.IsNullOrEmpty(Details) ? info : $"{info}\n{nameof(Details)}: {Details}";
    }

    public static bool operator ==(Distinction? left, Distinction? right) => Equals(left, right);

    public static bool operator !=(Distinction? left, Distinction? right) => !Equals(left, right);
}