using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers;
using ObjectsComparator.Helpers.Extensions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectsComparator.Comparator.Strategies.Implementations
{
    public sealed class CompareMembersStrategy : ICompareMembersStrategy
    {
        private static readonly MethodInfo CallGetDistinctions =
            typeof(CompareMembersStrategy).GetTypeInfo().GetDeclaredMethod(nameof(GetDistinctions))!;

        private static readonly ConcurrentDictionary<Type, MemberAccessor[]> CachedAccessors = new();
        private static readonly ConcurrentDictionary<Type, GetDistinctionDelegate> CachedDistinctionDelegates = new();

        private readonly Comparator _handler;

        public CompareMembersStrategy(Comparator handler)
        {
            _handler = handler;
        }

        public bool IsValid(Type member)
        {
            return member.IsClassAndNotString();
        }

        public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull
        {
            if (ReferenceEquals(expected, actual)) return DeepEqualityResult.None();
            var diff = DeepEqualityResult.Create();
            var type = expected.GetType();

            foreach (var accessor in CachedAccessors.GetOrAdd(type, CreateAccessors))
            {
                var actualPropertyPath = string.IsNullOrEmpty(propertyName)
                    ? accessor.Name
                    : $"{propertyName}.{accessor.Name}";

                var firstValue = accessor.Getter(expected);
                var secondValue = accessor.Getter(actual);

                var diffRes = CachedDistinctionDelegates
                    .GetOrAdd(accessor.MemberType, CreateDistinctionDelegate)
                    .Invoke(this, actualPropertyPath, firstValue, secondValue);

                if (diffRes.IsNotEmpty()) diff.AddRange(diffRes);
            }

            return diff;
        }

        private static MemberAccessor[] CreateAccessors(Type type)
        {
            return type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.MemberType is MemberTypes.Property or MemberTypes.Field)
                .Select(CreateAccessor)
                .Where(accessor => accessor is not null)
                .Cast<MemberAccessor>()
                .ToArray();
        }

        private static MemberAccessor? CreateAccessor(MemberInfo member)
        {
            switch (member)
            {
                case PropertyInfo { CanRead: true } property when property.GetIndexParameters().Length == 0:
                    var helper = PropertyHelper.Instance(property);
                    return new MemberAccessor(member.Name, helper.Property.PropertyType, helper.GetValue);
                case FieldInfo field:
                    return new MemberAccessor(member.Name, field.FieldType, field.GetValue);
                default:
                    return null;
            }
        }

        private static GetDistinctionDelegate CreateDistinctionDelegate(Type memberType)
        {
            var method = CallGetDistinctions.MakeGenericMethod(memberType);

            var strategyParameter = Expression.Parameter(typeof(CompareMembersStrategy), "strategy");
            var propertyParameter = Expression.Parameter(typeof(string), "propertyName");
            var expectedParameter = Expression.Parameter(typeof(object), "expected");
            var actualParameter = Expression.Parameter(typeof(object), "actual");

            var call = Expression.Call(
                strategyParameter,
                method,
                propertyParameter,
                Expression.Convert(expectedParameter, memberType),
                Expression.Convert(actualParameter, memberType));

            return Expression.Lambda<GetDistinctionDelegate>(
                call,
                strategyParameter,
                propertyParameter,
                expectedParameter,
                actualParameter).Compile();
        }

        public DeepEqualityResult GetDistinctions<T>(string propertyName, T expected, T actual)
        {
            return _handler.RulesHandler.GetFor(typeof(T))
                .Compare(expected, actual, propertyName);
        }

        private delegate DeepEqualityResult GetDistinctionDelegate(CompareMembersStrategy strategy, string propertyName,
            object? expected, object? actual);

        private sealed class MemberAccessor(string name, Type memberType, Func<object, object?> getter)
        {
            public string Name { get; } = name;
            public Type MemberType { get; } = memberType;
            public Func<object, object?> Getter { get; } = getter;
        }
    }
}