using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties
{
    public class MemberStrategy<T> : ICustomCompareValues
    {
        private static readonly ConcurrentDictionary<string, Delegate> Cache = new();

        private readonly Display _display;
        private readonly Expression<Func<T, T, bool>> _compareFunc;

        public MemberStrategy(Expression<Func<T, T, bool>> compareFunc, Display display)
        {
            _display = display;
            _compareFunc = compareFunc;
        }

        public DeepEqualityResult Compare<T1>(T1 expected, T1 actual, string propertyName) =>
            CastParameters<T1>(propertyName)(expected, actual)
                ? DeepEqualityResult.None()
                : DeepEqualityResult.Create(_display.GetDistinction(expected, actual, propertyName,
                    _compareFunc.GetLambdaString(expected, actual)));

        private Func<T1, T1, bool> CastParameters<T1>(string propertyName)
        {
            var key = $"{propertyName}:{_compareFunc}";
            return (Func<T1, T1, bool>)Cache.GetOrAdd(key, (k) =>
           {
               var castParameterExpressions = _compareFunc.Parameters.Select(p => Expression.Parameter(p.Type, p.Name)).ToList();
               var invocationExpression = Expression.Invoke(_compareFunc, castParameterExpressions);
               var lambda = Expression.Lambda(invocationExpression, castParameterExpressions);
               var func = lambda.Compile();
               return func;
           });
        }
    }
}