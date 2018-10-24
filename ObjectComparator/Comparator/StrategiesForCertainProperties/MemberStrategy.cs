using System;
using System.Linq.Expressions;
using ObjectComparator.Helpers.Extensions;

namespace ObjectComparator.Comparator.StrategiesForCertainProperties
{
    public class MemberStrategy<T> : IMemberStrategy
    {
        private readonly Display _display;

        public Expression<Func<T, T, bool>> CompareFunc { get; set; }

        public MemberStrategy(string propertyName, Expression<Func<T, T, bool>> compareFunc,
            Display display)
        {
            _display = display;
            MemberName = propertyName;
            CompareFunc = compareFunc;
        }

        public DistinctionsCollection Compare<T1>(T1 valueA, T1 valueB) => Compare(valueA, valueB, MemberName);

        public DistinctionsCollection Compare<T1>(T1 valueA, T1 valueB, string propertyName)
        {
            var a = (T) (object) valueA;
            var b = (T) (object) valueB;
            return CompareFunc.Compile()(a, b)
                ? new DistinctionsCollection()
                : new DistinctionsCollection(new[]
                {
                    _display.GetDistinction(a.ToString(), b.ToString(), MemberName,
                        BodyExpression.Get(CompareFunc).ToString())
                });
        }

        public string MemberName { get; set; }
    }
}