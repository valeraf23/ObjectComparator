using FluentAssertions;
using NUnit.Framework;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Tests.TestModels;
using System.Collections.Generic;
using System.Linq;

namespace ObjectsComparator.Tests;

[TestFixture]
public class CollectionComparisonTests
{
    [Test]
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

        var res = act.DeeplyEquals(exp);
        res.Should().NotBeEmpty();
        var diff = res.First();
        diff.ActualValue.Should().Be("ttt1");
        diff.ExpectedValue.Should().Be("ttt");
    }

    [Test]
    public void CompareIEnumerableProperty()
    {
        var act = new BuildingEnumerable
        {
            Address = "NY, First Street",
            ListOfAppNumbers = new[] { 32, 25, 14, 89 }
        };

        var exp = new BuildingEnumerable
        {
            Address = "NY, First Street",
            ListOfAppNumbers = new[] { 555, 25, 14, 89 }
        };

        exp.DeeplyEquals(act)[0].Should()
            .Be(new Distinction("BuildingEnumerable.ListOfAppNumbers[0]", 555, 32));
    }

    [Test]
    public void CompareIEnumerableImplementation()
    {
        var act = new StringList { "A", "B" };
        var exp = new StringList { "B", "C" };

        exp.DeeplyEquals(act)[0].Should()
            .Be(new Distinction("StringList[0]", "B", "A"));
    }

    [Test]
    public void CompareIEnumerableImplementationAsObject()
    {
        object act = new StringList { "A", "B" };
        object exp = new StringList { "B", "C" };

        exp.DeeplyEquals(act)[0].Should()
            .Be(new Distinction("StringList[0]", "B", "A"));
    }

    [Test]
    public void DeeplyEquals_ShouldReturnJoinedCollectionValuesDifference_WhenCollectionLengthsDiffer()
    {
        var actual = new GroupPortals
        {
            Portals = new List<int> { 1, 2, 3, 5 },
            Portals1 = new List<GroupPortals1>
            {
                new()
                {
                    Courses = new List<Course> { new() { Name = "test" } }
                }
            }
        };

        var expected = new GroupPortals
        {
            Portals = new List<int> { 1, 2, 3, 4, 7, 0 },
            Portals1 = new List<GroupPortals1>
            {
                new()
                {
                    Courses = new List<Course> { new() { Name = "test1" } }
                }
            }
        };

        var result = expected.DeeplyEquals(actual);
        result[0].Should()
            .Be(new Distinction("GroupPortals.Portals[3]", 4, 5));

        result[1].Should()
            .Be(new Distinction("GroupPortals.Portals[4]", 7, null, "Removed"));

        result[2].Should()
            .Be(new Distinction("GroupPortals.Portals[5]", 0, null, "Removed"));

        result[3].Should()
            .Be(new Distinction("GroupPortals.Portals1[0].Courses[0].Name", "test1", "test"));
    }

    [Test]
    public void DeeplyEquals_Collections_WithCustomStrategy_ShouldApplyToAllElements()
    {
        var expected = new List<VehicleDto>
        {
            new() { Id = 1, Model = "BMW", Description = "", InternalCode = "A" },
            new() { Id = 2, Model = "Audi", Description = null, InternalCode = "B" },
            new() { Id = 3, Model = null, Description = "Description", InternalCode = "C" }
        };

        var actual = new List<VehicleDto>
        {
            new() { Id = 1, Model = "BMW", Description = null, InternalCode = "A" },
            new() { Id = 2, Model = "Audi", Description = "", InternalCode = "B" },
            new() { Id = 3, Model = "", Description = "Description", InternalCode = "C" }
        };

        var result = expected.DeeplyEquals(actual,
            strategy => strategy
                .Set(x => x.Model, (exp, act) =>
                    (string.IsNullOrEmpty(exp) && string.IsNullOrEmpty(act)) || exp == act)
                .Set(x => x.Description, (exp, act) =>
                    (string.IsNullOrEmpty(exp) && string.IsNullOrEmpty(act)) || exp == act));

        result.Should().BeEmpty();
    }

    [Test]
    public void DeeplyEquals_Collections_DifferentTypes_WithCustomStrategy_ShouldWork()
    {
        var expected = new List<VehicleDto>
        {
            new() { Id = 1, Model = "", Description = "Test1", InternalCode = "A" },
            new() { Id = 2, Model = null, Description = "Test2", InternalCode = "B" }
        };

        var actual = new List<VehicleEntity>
        {
            new() { Id = 1, Model = null, Description = "Test1", InternalCode = "A" },
            new() { Id = 2, Model = "", Description = "Test2", InternalCode = "B" }
        };

        var result = expected.DeeplyEquals(actual,
            strategy => strategy
                .Set(x => x.Model, (exp, act) =>
                    (string.IsNullOrEmpty(exp) && string.IsNullOrEmpty(act)) || exp == act),
            options => options.AllowDifferentTypes());

        result.Should().BeEmpty();
    }

    [Test]
    public void DeeplyEquals_Collections_DifferentTypes_WithCustomStrategyAndIgnore_ShouldWork()
    {
        var expected = new List<VehicleDto>
        {
            new() { Id = 1, Model = "", Description = "Test", InternalCode = "ABC" },
            new() { Id = 2, Model = null, Description = "Test", InternalCode = "DEF" }
        };

        var actual = new List<VehicleEntity>
        {
            new() { Id = 1, Model = null, Description = "Test", InternalCode = "XYZ" },
            new() { Id = 2, Model = "", Description = "Test", InternalCode = "XYZ" }
        };

        var result = expected.DeeplyEquals(actual,
            strategy => strategy
                .Set(x => x.Model, (exp, act) =>
                    (string.IsNullOrEmpty(exp) && string.IsNullOrEmpty(act)) || exp == act),
            options => options.AllowDifferentTypes(),
            "InternalCode");

        result.Should().BeEmpty();
    }

    [Test]
    public void DeeplyEquals_Collections_DifferentTypes_ShouldReportDifferences()
    {
        var expected = new List<VehicleDto>
        {
            new() { Id = 1, Model = "BMW", Description = "Expected", InternalCode = "A" },
            new() { Id = 2, Model = "Audi", Description = "Same", InternalCode = "B" }
        };

        var actual = new List<VehicleEntity>
        {
            new() { Id = 1, Model = "Mercedes", Description = "Expected", InternalCode = "A" },
            new() { Id = 2, Model = "Audi", Description = "Same", InternalCode = "B" }
        };

        var result = expected.DeeplyEquals(actual,
            strategy => strategy
                .Set(x => x.Description, (exp, act) =>
                    (string.IsNullOrEmpty(exp) && string.IsNullOrEmpty(act)) || exp == act),
            options => options.AllowDifferentTypes());

        result.Should().HaveCount(1);
        result.First().Path.Should().Contain("[0]");
        result.First().Path.Should().EndWith("Model");
        result.First().ExpectedValue.Should().Be("BMW");
        result.First().ActualValue.Should().Be("Mercedes");
    }

    [Test]
    public void DeeplyEquals_Collections_NestedProperties_WithCustomStrategy_ShouldWork()
    {
        var expected = new List<StudentDto>
        {
            new()
            {
                Name = "",
                Age = 20,
                Courses = new[] { new CourseDto { Name = "Math", Credits = 3 } }
            }
        };

        var actual = new List<StudentEntity>
        {
            new()
            {
                Name = null,
                Age = 20,
                Courses = new[] { new CourseEntity { Name = "Math", Credits = 3 } }
            }
        };

        var result = expected.DeeplyEquals(actual,
            strategy => strategy
                .Set(x => x.Name, (exp, act) =>
                    (string.IsNullOrEmpty(exp) && string.IsNullOrEmpty(act)) || exp == act),
            options => options.AllowDifferentTypes());

        result.Should().BeEmpty();
    }
}