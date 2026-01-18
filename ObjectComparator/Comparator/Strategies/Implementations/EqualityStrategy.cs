using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace ObjectsComparator.Comparator.Strategies.Implementations;

public sealed class EqualityStrategy : IEqualityStrategy
{
    private const string Details = "== (Equality Operator)";
    private const string OpEquality = "op_Equality";
    private static readonly ConcurrentDictionary<Type, MethodInfo> Cache = new();

    public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull
    {
        var member = expected.GetType();
        Cache.TryGetValue(member, out var del);

        bool EqualsOperatorResult(T exp, T act)
        {
            return (bool?)del?.Invoke(null, new[] { exp, (object)act }) ?? false;
        }

        return DeepEqualityResult.CreateFor(propertyName, expected, actual, Details).WhenNot(EqualsOperatorResult);
    }

    public bool IsValid(Type member)
    {
        var equalsOperator = member.GetMethods().FirstOrDefault(m => m.Name == OpEquality);
        var res = equalsOperator is not null;
        if (res)
        {
            Cache.TryAdd(member, equalsOperator!);
        }

        return res;
    }
}