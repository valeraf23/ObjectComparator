using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Tests.TestModels;
using System.Collections.Generic;

namespace PerformanceTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<PerformanceTests>();
        }
    }


    [MemoryDiagnoser]
    public class PerformanceTests
    {

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
    }
}
