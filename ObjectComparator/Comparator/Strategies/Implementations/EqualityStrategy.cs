using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace ObjectsComparator.Comparator.Strategies.Implementations;

internal sealed class EqualityStrategy : IEqualityStrategy
{
    internal static readonly EqualityStrategy Instance = new();

    private const string Details = "== (Equality Operator)";
    private const string OpEquality = "op_Equality";

    private static readonly ConcurrentDictionary<Type, Func<object, object, bool>?> OperatorCache = new();

    public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull
    {
        var equalityOperator = OperatorCache.GetOrAdd(typeof(T), CreateEqualityDelegate)
                               ?? OperatorCache.GetOrAdd(expected.GetType(), CreateEqualityDelegate);

        return equalityOperator is not null && equalityOperator(expected, actual)
            ? DeepEqualityResult.None()
            : DeepEqualityResult.Create(new Distinction(propertyName, expected, actual, Details));
    }

    public bool IsValid(Type member)
    {
        return OperatorCache.GetOrAdd(member, CreateEqualityDelegate) is not null;
    }

    private static Func<object, object, bool>? CreateEqualityDelegate(Type type)
    {
        var method = type.GetMethods().FirstOrDefault(m => m.Name == OpEquality);
        if (method is null)
        {
            return null;
        }

        var parameters = method.GetParameters();
        var expectedParameter = Expression.Parameter(typeof(object), "expected");
        var actualParameter = Expression.Parameter(typeof(object), "actual");

        Expression body = Expression.Call(method,
            Expression.Convert(expectedParameter, parameters[0].ParameterType),
            Expression.Convert(actualParameter, parameters[1].ParameterType));

        if (body.Type != typeof(bool))
        {
            body = Expression.Convert(body, typeof(bool));
        }

        return Expression.Lambda<Func<object, object, bool>>(body, expectedParameter, actualParameter).Compile();
    }
}
