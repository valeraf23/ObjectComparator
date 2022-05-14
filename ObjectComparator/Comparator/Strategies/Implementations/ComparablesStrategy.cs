using System;
using System.Linq;
using System.Reflection;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;

namespace ObjectsComparator.Comparator.Strategies.Implementations
{
    public sealed class ComparablesStrategy : IComparablesStrategy
    {
        private static readonly Type ObjectType = typeof(object);
        private const string MethodName = "CompareTo";
        private static readonly Type Type = typeof(IComparable<>);
        private static readonly string Details = $"used_{MethodName}()";

        private static bool Predicate(MethodInfo methodInfo) =>
            methodInfo.Name == MethodName
            && methodInfo.GetParameters().All(d => d.ParameterType != ObjectType);

        public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull =>
            DeepEqualityResult.CreateFor(propertyName, expected, actual, Details).WhenNot(IsEqual);

        private bool IsEqual<T>(T expected, T actual) where T : notnull =>
            expected is IComparable<T> compareToMethod && compareToMethod.CompareTo(actual) == 0
            || ForNullable(expected, actual);

        private static bool ForNullable<T>(T expected, T actual) where T : notnull
        {
            var compareToMethod = expected.GetType().GetMethods().FirstOrDefault(Predicate);
            if (compareToMethod is null)
                throw new Exception("Something has gone wrong when trying to find realization IComparable<>");

            var compareToResult = (int) compareToMethod.Invoke(expected, new[] {(object) actual})!;
            return compareToResult == 0;
        }

        public bool IsValid(Type member) => member.ImplementsGenericInterface(Type);
    }
}