using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;
using System;

namespace ObjectsComparator.Comparator.Strategies.Implementations;

internal sealed class OverridesEqualsStrategy : IOverridesEqualsStrategy
{
    internal static readonly OverridesEqualsStrategy Instance = new();

    private const string Details = "Was used override 'Equals()'";

    public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull
    {
        // Intentionally virtual Equals(object): IsValid matched on a declared Equals override,
        // and the runtime type may differ from T.
        return expected.Equals(actual)
            ? DeepEqualityResult.None()
            : DeepEqualityResult.Create(new Distinction(propertyName, expected, actual, Details));
    }

    public bool IsValid(Type member)
    {
        return member.IsOverridesEqualsMethod();
    }
}