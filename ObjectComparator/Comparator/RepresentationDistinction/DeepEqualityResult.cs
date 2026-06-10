using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ObjectsComparator.Comparator.RepresentationDistinction;

[DebuggerDisplay("{" + nameof(ToString) + "()}")]
public sealed class DeepEqualityResult : IEnumerable<Distinction>
{
    private List<Distinction>? _list;

    private DeepEqualityResult()
    {
    }

    private DeepEqualityResult(int capacity)
    {
        if (capacity > 0)
        {
            _list = new List<Distinction>(capacity);
        }
    }

    private DeepEqualityResult(IEnumerable<Distinction> collection)
    {
        _list = new List<Distinction>(collection);
    }

    public Distinction this[int i]
    {
        get => _list is null ? throw new ArgumentOutOfRangeException(nameof(i)) : _list[i];
        set => Add(value);
    }

    public IEnumerator<Distinction> GetEnumerator()
    {
        return _list?.GetEnumerator()
               ?? Enumerable.Empty<Distinction>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static DeepEqualityResult None()
    {
        return new DeepEqualityResult(0);
    }

    public static DeepEqualityResult Create()
    {
        return new DeepEqualityResult();
    }

    public static DeepEqualityResult Create(string name, object expectedValue, object actuallyValue)
    {
        return Create(new Distinction(name, expectedValue, actuallyValue));
    }

    public static DeepEqualityResult Create(IEnumerable<Distinction> collection)
    {
        return new DeepEqualityResult(collection);
    }

    public static DeepEqualityResult Create(Distinction distinction)
    {
        return new DeepEqualityResult(1) { distinction };
    }

    public static ForDistinctionsBuilder<T> CreateFor<T>(string name, T expectedValue,
        T actuallyValue) where T : notnull
    {
        return new ForDistinctionsBuilder<T>(name, expectedValue, actuallyValue);
    }

    public static ForDistinctionsBuilder<T> CreateFor<T>(string name, T expectedValue,
        T actuallyValue, string details) where T : notnull
    {
        return new ForDistinctionsBuilder<T>(name, expectedValue, actuallyValue, details);
    }

    public DeepEqualityResult Add(Distinction input)
    {
        (_list ??= new List<Distinction>()).Add(input);
        return this;
    }

    public bool IsEmpty()
    {
        return _list is null || _list.Count == 0;
    }

    public bool IsNotEmpty()
    {
        return !IsEmpty();
    }

    public int Count()
    {
        return _list?.Count ?? 0;
    }

    public DeepEqualityResult AddRange(DeepEqualityResult collection)
    {
        var source = collection._list;
        if (source is { Count: > 0 })
        {
            (_list ??= new List<Distinction>(source.Count)).AddRange(source);
        }

        return this;
    }

    public override string ToString()
    {
        if (IsEmpty())
        {
            return "Objects are deeply equal";
        }


        return _list!.Aggregate(new StringBuilder(),
            (sb, distinction) =>
            {
                sb.AppendLine(distinction.ToString());
                sb.AppendLine();
                return sb;
            },
            sb => sb.ToString()).TrimEnd();
    }

    public static implicit operator bool(DeepEqualityResult deepEqualResult)
    {
        return deepEqualResult.IsEmpty();
    }
}