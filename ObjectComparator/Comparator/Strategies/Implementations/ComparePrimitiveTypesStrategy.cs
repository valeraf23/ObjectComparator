using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;
using System;
using System.Collections.Generic;

namespace ObjectsComparator.Comparator.Strategies.Implementations;

internal sealed class ComparePrimitiveTypesStrategy : ICompareStructStrategy
{
    internal static readonly ComparePrimitiveTypesStrategy Instance = new();

    public bool IsValid(Type member)
    {
        return member.IsPrimitiveOrString();
    }

    public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull
    {
        return EqualityComparer<T>.Default.Equals(expected, actual)
            ? DeepEqualityResult.None()
            : DeepEqualityResult.Create(new Distinction(propertyName, expected, actual));
    }
}