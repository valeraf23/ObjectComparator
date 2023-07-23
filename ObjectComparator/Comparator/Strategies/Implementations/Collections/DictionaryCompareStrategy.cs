using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ObjectsComparator.Comparator.RepresentationDistinction;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections
{
    public class DictionaryCompareStrategy : BaseCollectionsCompareStrategy
    {
        private static readonly MethodInfo CompareMethod = typeof(DictionaryCompareStrategy).GetTypeInfo()
            .GetDeclaredMethod(nameof(CompareDictionary))!;

        public DictionaryCompareStrategy(Comparator comparator) : base(comparator)
        {
        }

        public DeepEqualityResult CompareDictionary<TKey, TValue>(IDictionary<TKey, TValue> expected,
            IDictionary<TKey, TValue> actual, string propertyName) where TKey : notnull
        {
            if (expected.Count != actual.Count)
                return DeepEqualityResult.Create("Dictionary has different length", expected.Count, actual.Count);
            var diff = DeepEqualityResult.Create();
            foreach (var (key, value) in expected)
            {
                if (!actual.TryGetValue(key, out var secondValue))
                    diff.Add(new Distinction(key.ToString(), "Should be", "Does not exist"));

                var diffRes = RulesHandler.GetFor(typeof(TValue))
                    .Compare(value, secondValue, $"{propertyName}[{key}]");

                diff.AddRange(diffRes);
            }

            return diff;
        }

        public override DeepEqualityResult Compare<T>(T expected, T actual, string propertyName)
        {
            var genericArguments = GetGenericArguments(typeof(T));
            return (DeepEqualityResult) CompareMethod.MakeGenericMethod(genericArguments)
                .Invoke(this, new[] {(object) expected, actual, propertyName})!;
        }

        private static Type[] GetGenericArguments(Type type)
        {
            return type.GetInterfaces().Prepend(type)
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                .Select(i => i.GetGenericArguments()).First();
        }

        public override bool IsValid(Type member)
        {
            return member.GetInterfaces().Prepend(member)
                .Where(i => i.IsGenericType)
                .Select(i => i.GetGenericTypeDefinition())
                .Contains(typeof(IDictionary<,>));
        }
    }
}