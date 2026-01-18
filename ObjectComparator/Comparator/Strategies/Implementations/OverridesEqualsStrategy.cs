using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;
using System;

namespace ObjectsComparator.Comparator.Strategies.Implementations;

public sealed class OverridesEqualsStrategy : IOverridesEqualsStrategy
{
    private const string Details = "Was used override 'Equals()'";

    public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull
    {
        return DeepEqualityResult
            .CreateFor(propertyName, expected, actual, Details).WhenNot((ex, act) => ex.Equals(act));
    }

    public bool IsValid(Type member)
    {
        return member.IsOverridesEqualsMethod();
    }
}