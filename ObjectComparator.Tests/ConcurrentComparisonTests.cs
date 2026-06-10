using FluentAssertions;
using NUnit.Framework;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Tests.TestModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ObjectsComparator.Tests;

[TestFixture]
public class ConcurrentComparisonTests
{
    [Test]
    public void Parallel_Comparisons_Across_Varied_Types_Are_Consistent()
    {
        Parallel.For(0, 200, i =>
        {
            var expectedPoco = new SomeClass { Foo = "foo" + i };
            var actualPoco = new SomeClass { Foo = "foo" + i };
            expectedPoco.DeeplyEquals(actualPoco).IsEmpty().Should().BeTrue();

            var expectedList = new List<int> { 1, 2, i };
            var actualList = new List<int> { 1, 2, i + 1 };
            expectedList.DeeplyEquals(actualList).IsNotEmpty().Should().BeTrue();

            var expectedDict = new Dictionary<string, Book>
            {
                ["a"] = new() { Pages = i, Text = "text" }
            };
            var actualDict = new Dictionary<string, Book>
            {
                ["a"] = new() { Pages = i, Text = "text" }
            };
            expectedDict.DeeplyEquals(actualDict).IsEmpty().Should().BeTrue();

            var expectedBuilding = new BuildingList
            {
                Address = "addr",
                ListOfAppNumbers = new List<int> { i }
            };
            var actualBuilding = new BuildingList
            {
                Address = "addr",
                ListOfAppNumbers = new List<int> { i }
            };
            expectedBuilding.DeeplyEquals(actualBuilding).IsEmpty().Should().BeTrue();
        });
    }
}
