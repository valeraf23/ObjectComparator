using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace ObjectsComparator.Comparator.Strategies.Implementations;

public sealed class ComparablesStrategy : IComparablesStrategy
{
    private const string MethodName = "CompareTo";
    private const string Details = $"used_{MethodName}()";
    private static readonly Type ObjectType = typeof(object);
    private static readonly Type Type = typeof(IComparable<>);
    private readonly bool _structsOnly;

    public ComparablesStrategy(bool structsOnly = false)
    {
        _structsOnly = structsOnly;
    }

    public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull
    {
        return DeepEqualityResult.CreateFor(propertyName, expected, actual, Details).WhenNot(IsEqual);
    }

    public bool IsValid(Type member)
    {
        if (_structsOnly)
        {
            return !member.IsClass && member.ImplementsGenericInterface(Type);
        }

        return member.ImplementsGenericInterface(Type);
    }

    private static bool Predicate(MethodInfo methodInfo)
    {
        return methodInfo.Name == MethodName
               && methodInfo.GetParameters().All(d => d.ParameterType != ObjectType);
    }

    private bool IsEqual<T>(T expected, T actual) where T : notnull
    {
        return expected is IComparable<T> compareToMethod && compareToMethod.CompareTo(actual) == 0
               || ForNullable(expected, actual);
    }

    private static bool ForNullable<T>(T expected, T actual) where T : notnull
    {
        var compareToMethod = expected.GetType().GetMethods().FirstOrDefault(Predicate);
        if (compareToMethod is null)
        {
            throw new Exception("Something has gone wrong when trying to find realization IComparable<>");
        }

        var compareToResult = (int)compareToMethod.Invoke(expected, new[] { (object)actual })!;
        return compareToResult == 0;
    }
}