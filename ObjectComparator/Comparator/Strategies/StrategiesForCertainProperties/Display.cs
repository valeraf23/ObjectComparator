using ObjectsComparator.Comparator.RepresentationDistinction;

namespace ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties
{
    public sealed class Display
    {
        public string Expected { get; set; } = string.Empty;
        public string Actually { get; set; } = string.Empty;

        public Distinction GetDistinction<T>(T expected, T actual, string propertyName, string lambdaExpression)
        {
            var expectedValue = string.IsNullOrEmpty(Expected) ? ReplaceNull(expected) : Expected;
            var actuallyValue = string.IsNullOrEmpty(Actually) ? ReplaceNull(actual) : Actually;
            return new Distinction(propertyName, expectedValue, actuallyValue, lambdaExpression);
        }

        private static string ReplaceNull(object? value) => value is null ? "null" : $"{value}";

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
    }
}