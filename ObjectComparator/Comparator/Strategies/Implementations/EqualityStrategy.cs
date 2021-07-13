using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Strategies.Implementations
{
    public sealed class EqualityStrategy : IEqualityStrategy
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> Cache = new();

        private const string Details = "== (Equality Operator)";
        private const string OpEquality = "op_Equality";

        public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull
        {
            var member = expected.GetType();
            Cache.TryGetValue(member, out var del);
            bool EqualsOperatorResult(T exp, T act) => (bool) del?.Invoke(null, new[] {exp, (object) act})!;
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
}