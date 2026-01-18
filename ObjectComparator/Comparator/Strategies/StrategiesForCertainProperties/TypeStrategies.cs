using ObjectsComparator.Comparator.Strategies.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties;

public sealed class TypeStrategies : IEnumerable<KeyValuePair<Type, ICustomCompareValues>>
{
    private readonly IDictionary<Type, ICustomCompareValues> _strategies =
        new Dictionary<Type, ICustomCompareValues>();

    public IEnumerator<KeyValuePair<Type, ICustomCompareValues>> GetEnumerator() => _strategies.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Sets a custom comparison strategy for all properties of the specified type.
    /// </summary>
    /// <param name="type">The type to apply the strategy to.</param>
    /// <param name="comparer">The comparison function that returns true if values are considered equal.</param>
    /// <returns>The TypeStrategies instance for method chaining.</returns>
    public TypeStrategies Set(Type type, Func<object?, object?, bool> comparer)
    {
        return Set(type, comparer, new Display());
    }

    /// <summary>
    /// Sets a custom comparison strategy for all properties of the specified type with custom display options.
    /// </summary>
    /// <param name="type">The type to apply the strategy to.</param>
    /// <param name="comparer">The comparison function that returns true if values are considered equal.</param>
    /// <param name="displayBuilder">A function to configure the display options.</param>
    /// <returns>The TypeStrategies instance for method chaining.</returns>
    public TypeStrategies Set(Type type, Func<object?, object?, bool> comparer, Func<Display, Display> displayBuilder)
    {
        if (_strategies.ContainsKey(type))
            throw new InvalidOperationException($"Strategy for type \"{type.Name}\" has already been added");

        _strategies.Add(type, new TypeMemberStrategy(comparer, displayBuilder(new Display())));
        return this;
    }

    /// <summary>
    /// Sets a custom comparison strategy for all properties of the specified type with custom display options.
    /// </summary>
    /// <param name="type">The type to apply the strategy to.</param>
    /// <param name="comparer">The comparison function that returns true if values are considered equal.</param>
    /// <param name="display">The display options.</param>
    /// <returns>The TypeStrategies instance for method chaining.</returns>
    public TypeStrategies Set(Type type, Func<object?, object?, bool> comparer, Display display)
    {
        if (_strategies.ContainsKey(type))
            throw new InvalidOperationException($"Strategy for type \"{type.Name}\" has already been added");

        _strategies.Add(type, new TypeMemberStrategy(comparer, display));
        return this;
    }

    /// <summary>
    /// Sets a strongly-typed custom comparison strategy for all properties of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to apply the strategy to.</typeparam>
    /// <param name="comparer">The comparison expression that returns true if values are considered equal.</param>
    /// <returns>The TypeStrategies instance for method chaining.</returns>
    public TypeStrategies Set<T>(Expression<Func<T, T, bool>> comparer)
    {
        return Set<T>(comparer, new Display());
    }

    /// <summary>
    /// Sets a strongly-typed custom comparison strategy for all properties of the specified type with custom display options.
    /// </summary>
    /// <typeparam name="T">The type to apply the strategy to.</typeparam>
    /// <param name="comparer">The comparison expression that returns true if values are considered equal.</param>
    /// <param name="displayBuilder">A function to configure the display options.</param>
    /// <returns>The TypeStrategies instance for method chaining.</returns>
    public TypeStrategies Set<T>(Expression<Func<T, T, bool>> comparer, Func<Display, Display> displayBuilder)
    {
        var type = typeof(T);
        if (_strategies.ContainsKey(type))
            throw new InvalidOperationException($"Strategy for type \"{type.Name}\" has already been added");

        _strategies.Add(type, new MemberStrategy<T>(comparer, displayBuilder(new Display())));
        return this;
    }

    /// <summary>
    /// Sets a strongly-typed custom comparison strategy for all properties of the specified type with custom display options.
    /// </summary>
    /// <typeparam name="T">The type to apply the strategy to.</typeparam>
    /// <param name="comparer">The comparison expression that returns true if values are considered equal.</param>
    /// <param name="display">The display options.</param>
    /// <returns>The TypeStrategies instance for method chaining.</returns>
    public TypeStrategies Set<T>(Expression<Func<T, T, bool>> comparer, Display display)
    {
        var type = typeof(T);
        if (_strategies.ContainsKey(type))
            throw new InvalidOperationException($"Strategy for type \"{type.Name}\" has already been added");

        _strategies.Add(type, new MemberStrategy<T>(comparer, display));
        return this;
    }

    internal Dictionary<Type, ICustomCompareValues> ToDictionary()
    {
        return new Dictionary<Type, ICustomCompareValues>(_strategies);
    }
}