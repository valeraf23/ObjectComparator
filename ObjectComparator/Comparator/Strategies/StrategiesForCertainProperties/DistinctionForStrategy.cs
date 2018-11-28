using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Helpers.GuardArgument;

namespace ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties
{
    public class DistinctionForStrategy : Distinction
    {
        public DistinctionForStrategy(string lambdaExpression, string name, object expectedValue, object actuallyValue)
            : base(name, expectedValue, actuallyValue)
        {
            GuardArgument.ArgumentIsNotNull(lambdaExpression);
            LambdaExpression = lambdaExpression;
        }

        public string LambdaExpression { get; }

        public override string ToString()
        {
            return
                $"Name: {Name}\n Expected Value :{ExpectedValue}\n Actually Value :{ActuallyValue}\n LambdaExpression :{LambdaExpression}";
        }
    }
}