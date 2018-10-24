namespace ObjectComparator.Comparator.StrategiesForCertainProperties
{
    public sealed class Display
    {
        public string Expected { get; set; }
        public string Actually { get; set; }

        public Distinction GetDistinction(string expected, string actual, string propertyName, string lambdaExpression)
        {
            var expectedValue = string.IsNullOrEmpty(Expected) ? expected : Expected;
            var actuallyValue = string.IsNullOrEmpty(Actually) ? actual : Actually;
            return new DistinctionForStrategy(lambdaExpression, propertyName, expectedValue, actuallyValue);
        }

        public Distinction GetDistinction<T>(T expected, T actual, string propertyName, string lambdaExpression)
            where T : struct => GetDistinction(expected.ToString(), actual.ToString(), propertyName, lambdaExpression);
    }
}