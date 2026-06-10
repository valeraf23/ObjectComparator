using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Tests.TestModels;
using System.Collections.Generic;

namespace PerformanceTests;

internal class Program
{
    private static void Main(string[] args)
    {
        BenchmarkRunner.Run<PerformanceTests>();
    }
}

public class SimplePoco
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Score { get; set; }
    public bool Active { get; set; }
}

public class ComplexKey
{
    public int Part1 { get; set; }
    public string Part2 { get; set; }

    public override bool Equals(object obj)
    {
        return obj is ComplexKey other && Part1 == other.Part1 && Part2 == other.Part2;
    }

    public override int GetHashCode()
    {
        return Part1 ^ (Part2?.GetHashCode() ?? 0);
    }
}

public class GraphNode
{
    public string Name { get; set; }
    public int Value { get; set; }
    public GraphNode Child { get; set; }
}

[MemoryDiagnoser]
[ShortRunJob]
public class PerformanceTests
{
    private SimplePoco _simpleExpected;
    private SimplePoco _simpleActualEqual;
    private SimplePoco _simpleActualDistinct;
    private List<int> _largeIntListExpected;
    private List<int> _largeIntListActual;
    private List<SomeClass> _largeObjectListExpected;
    private List<SomeClass> _largeObjectListActual;
    private Dictionary<string, Book> _dictStringKeyExpected;
    private Dictionary<string, Book> _dictStringKeyActual;
    private Dictionary<ComplexKey, Book> _dictComplexKeyExpected;
    private Dictionary<ComplexKey, Book> _dictComplexKeyActual;
    private GraphNode _deepGraphExpected;
    private GraphNode _deepGraphActual;

    [GlobalSetup]
    public void Setup()
    {
        _simpleExpected = new SimplePoco { Id = 1, Name = "name", Score = 1.5, Active = true };
        _simpleActualEqual = new SimplePoco { Id = 1, Name = "name", Score = 1.5, Active = true };
        _simpleActualDistinct = new SimplePoco { Id = 1, Name = "other", Score = 1.5, Active = true };

        _largeIntListExpected = new List<int>(10_000);
        _largeIntListActual = new List<int>(10_000);
        for (var i = 0; i < 10_000; i++)
        {
            _largeIntListExpected.Add(i);
            _largeIntListActual.Add(i);
        }

        _largeIntListActual[5_000] = -1;

        _largeObjectListExpected = new List<SomeClass>(1_000);
        _largeObjectListActual = new List<SomeClass>(1_000);
        for (var i = 0; i < 1_000; i++)
        {
            _largeObjectListExpected.Add(new SomeClass { Foo = "foo" + i });
            _largeObjectListActual.Add(new SomeClass { Foo = "foo" + i });
        }

        _largeObjectListActual[500].Foo = "changed";

        _dictStringKeyExpected = new Dictionary<string, Book>(1_000);
        _dictStringKeyActual = new Dictionary<string, Book>(1_000);
        for (var i = 0; i < 1_000; i++)
        {
            _dictStringKeyExpected["key" + i] = new Book { Pages = i, Text = "text" + i };
            _dictStringKeyActual["key" + i] = new Book { Pages = i, Text = "text" + i };
        }

        _dictStringKeyActual["key500"].Text = "changed";

        _dictComplexKeyExpected = new Dictionary<ComplexKey, Book>(100);
        _dictComplexKeyActual = new Dictionary<ComplexKey, Book>(100);
        for (var i = 0; i < 100; i++)
        {
            _dictComplexKeyExpected[new ComplexKey { Part1 = i, Part2 = "p" + i }] =
                new Book { Pages = i, Text = "text" + i };
            _dictComplexKeyActual[new ComplexKey { Part1 = i, Part2 = "p" + i }] =
                new Book { Pages = i, Text = "text" + i };
        }

        _dictComplexKeyActual[new ComplexKey { Part1 = 50, Part2 = "p50" }].Text = "changed";

        _deepGraphExpected = BuildGraph(5);
        _deepGraphActual = BuildGraph(5);
    }

    private static GraphNode BuildGraph(int depth)
    {
        var root = new GraphNode { Name = "level0", Value = 0 };
        var current = root;
        for (var i = 1; i < depth; i++)
        {
            current.Child = new GraphNode { Name = "level" + i, Value = i };
            current = current.Child;
        }

        return root;
    }

    [Benchmark]
    public void Set_Strategy_For_Collection_Ref_Type()
    {
        const string data = "actual";
        var act = new ClassA
        {
            One = "f",
            Two = 5,
            ArrayThird = new[] { "sss", "ggg" },
            InnerClass = new[] { new SomeClass { Foo = data }, new SomeClass { Foo = data } }
        };

        var exp = new ClassA
        {
            One = "f",
            Two = 5,
            ArrayThird = new[] { "sss", "ggg" },
            InnerClass = new[] { new SomeClass { Foo = "some" }, new SomeClass { Foo = data } }
        };

        act.DeeplyEquals(exp, str => str.Set(x => x.InnerClass[0].Foo,
            (s, s1) => s == data));
    }

    [Benchmark]
    public void Enumerable()
    {
        var act = new ClassC
        {
            One = "f",
            Two = 5,
            ArrayThird = new[] { "sss", "ggg" },
            InnerClass = new HashSet<string> { "ttt", "ttt2" }
        };

        var exp = new ClassC
        {
            One = "f",
            Two = 5,
            ArrayThird = new[] { "sss", "ggg" },
            InnerClass = new HashSet<string> { "ttt1", "ttt2" }
        };

        act.DeeplyEquals(exp);
    }

    [Benchmark]
    public void EnumerableValueType()
    {
        var act = new BuildingList
        {
            Address = "dsfd",
            ListOfAppNumbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }
        };

        var exp = new BuildingList
        {
            Address = "dsfd",
            ListOfAppNumbers = new List<int> { 1, 2, 3, 4, 5, 61, 7, 8, 91, 10, 11, 12 }
        };

        act.DeeplyEquals(exp);
    }

    [Benchmark]
    public DeepEqualityResult RepeatedSimpleObject_Equal()
    {
        return _simpleExpected.DeeplyEquals(_simpleActualEqual);
    }

    [Benchmark]
    public DeepEqualityResult RepeatedSimpleObject_Distinct()
    {
        return _simpleExpected.DeeplyEquals(_simpleActualDistinct);
    }

    [Benchmark]
    public DeepEqualityResult LargeIntList()
    {
        return _largeIntListExpected.DeeplyEquals(_largeIntListActual);
    }

    [Benchmark]
    public DeepEqualityResult LargeObjectList()
    {
        return _largeObjectListExpected.DeeplyEquals(_largeObjectListActual);
    }

    [Benchmark]
    public DeepEqualityResult Dictionary_StringKey()
    {
        return _dictStringKeyExpected.DeeplyEquals(_dictStringKeyActual);
    }

    [Benchmark]
    public DeepEqualityResult Dictionary_ComplexKey()
    {
        return _dictComplexKeyExpected.DeeplyEquals(_dictComplexKeyActual);
    }

    [Benchmark]
    public DeepEqualityResult DeepGraph()
    {
        return _deepGraphExpected.DeeplyEquals(_deepGraphActual);
    }

    [Benchmark]
    public DeepEqualityResult SameReference()
    {
        return _deepGraphExpected.DeeplyEquals(_deepGraphExpected);
    }
}
