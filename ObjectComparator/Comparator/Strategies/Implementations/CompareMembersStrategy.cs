using System;
using System.Linq;
using System.Reflection;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers;
using ObjectsComparator.Helpers.Extensions;

namespace ObjectsComparator.Comparator.Strategies.Implementations
{
    public sealed class CompareMembersStrategy : ICompareMembersStrategy
    {
        private static readonly Type DefaultType = typeof(object);

        private readonly Comparator _handler;

        public CompareMembersStrategy(Comparator handler) => _handler = handler;

        private static readonly MethodInfo CallGetDistinctions =
            typeof(CompareMembersStrategy).GetTypeInfo().GetDeclaredMethod(nameof(GetDistinctions))!;

        public bool IsValid(Type member) => member.IsClassAndNotString();

        public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull
        {
            if (ReferenceEquals(expected, actual)) return DeepEqualityResult.None();
            var diff = DeepEqualityResult.Create();
            var type = expected.GetType();

            foreach (var mi in type.GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(x =>
                x.MemberType is MemberTypes.Property or MemberTypes.Field))
            {
                var name = mi.Name;
                var actualPropertyPath = string.IsNullOrEmpty(propertyName)?  mi.Name: $"{propertyName}.{mi.Name}";

                object? firstValue = null;
                object? secondValue = null;
                Type memberType = DefaultType;

                switch (mi.MemberType)
                {
                    case MemberTypes.Field:
                        var field = type.GetField(name);
                        firstValue = field!.GetValue(expected);
                        secondValue = field.GetValue(actual);
                        memberType = field.FieldType;
                        break;
                    case MemberTypes.Property:
                        var del = PropertyHelper.Instance(type.GetProperty(name)!);
                        firstValue = del.GetValue(expected);
                        secondValue = del.GetValue(actual);
                        memberType = del.Property.PropertyType;
                        break;
                }

                var diffRes = (DeepEqualityResult) CallGetDistinctions.MakeGenericMethod(memberType)
                    .Invoke(this, new[] {actualPropertyPath, firstValue, secondValue})!;

                if (diffRes.IsNotEmpty()) diff.AddRange(diffRes);
            }

            return diff;
        }

        public DeepEqualityResult GetDistinctions<T>(string propertyName, T expected, T actual) =>
            _handler.RulesHandler.GetFor(typeof(T))
                .Compare(expected, actual, propertyName);
    }
}