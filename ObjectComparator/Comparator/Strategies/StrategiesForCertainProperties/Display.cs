using ObjectsComparator.Comparator.RepresentationDistinction;

namespace ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties;

public sealed class Display
{
    private bool _preserveOriginalValues;

    public string Expected { get; set; } = string.Empty;
    public string Actually { get; set; } = string.Empty;

    public Distinction GetDistinction<T>(T expected, T actual, string propertyName, string lambdaExpression)
    {
        object? expectedValue = string.IsNullOrEmpty(Expected)
            ? _preserveOriginalValues
                ? expected
                : ReplaceNull(expected)
            : Expected;
        object? actuallyValue = string.IsNullOrEmpty(Actually)
            ? _preserveOriginalValues
                ? actual
                : ReplaceNull(actual)
            : Actually;
        return new Distinction(propertyName, expectedValue, actuallyValue, lambdaExpression);
    }

    private static string ReplaceNull(object? value)
    {
        return value is null ? "null" : $"{value}";
    }

    public Display SetExpectedInformation(string text)
    {
        Expected = text;
        return this;
    }

    public Display SetActuallyInformation(string text)
    {
        Actually = text;
        return this;
    }

    /// <summary>
    /// Preserves original expected and actual values in the distinction.
    /// </summary>
    public Display KeepOriginalValues()
    {
        _preserveOriginalValues = true;
        return this;
    }
}
