using System;
using System.Linq.Expressions;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;

namespace ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties
{
    public class MemberStrategy<T> : ICompareValues
    {
        private readonly Display _display;

        public MemberStrategy(Expression<Func<T, T, bool>> compareFunc,
            Display display)
        {
            _display = display;
            CompareFunc = compareFunc;
        }

        public Expression<Func<T, T, bool>> CompareFunc { get; set; }

        public Distinctions Compare<T1>(T1 expected, T1 actual, string propertyName)
        {
            var a = (T) (object) expected;
            var b = (T) (object) actual;
            return CompareFunc.Compile()(a, b)
                ? new Distinctions()
                : new Distinctions(new[]
                {
                    _display.GetDistinction(a, b, propertyName,
                        BodyExpression.Get(CompareFunc).ToString())
                });
        }
    }
}