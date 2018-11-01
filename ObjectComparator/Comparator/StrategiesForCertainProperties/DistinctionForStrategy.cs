using ObjectsComparator.Helpers.GuardArgument;

namespace ObjectsComparator.Comparator.StrategiesForCertainProperties
{
    public class DistinctionForStrategy : Distinction
    {
        public string LambdaExpression { get; }

        public DistinctionForStrategy(string lambdaExpression, string name, object expectedValue, object actuallyValue)
            : base(name, expectedValue, actuallyValue)
        {
            GuardArgument.ArgumentIsNotNull(lambdaExpression);
            LambdaExpression = lambdaExpression;
        }

        public override string ToString() =>
            $"Name: {Name}\n Expected Value :{ExpectedValue}\n Actually Value :{ActuallyValue}\n LambdaExpression :{LambdaExpression}";
    }
}