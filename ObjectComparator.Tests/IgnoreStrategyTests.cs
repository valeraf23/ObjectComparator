using FluentAssertions;
using NUnit.Framework;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Tests.TestModels;
using System.Collections.Generic;

namespace ObjectsComparator.Tests;

[TestFixture]
public class IgnoreStrategyTests
{
    [Test]
    public void Set_StrategyIgnore()
    {
        var act = new Student
        {
            Name = "StudentName",
            Age = 1,
            Courses = new[] { new Course { Name = "CourseName" } }
        };

        var exp = new Student
        {
            Name = "StudentName1",
            Age = 1,
            Courses = new[] { new Course { Name = "CourseName1" } }
        };

        var res = act.DeeplyEquals(exp, propName => propName.EndsWith("Name"));
        res.Should().BeEmpty();
    }

    [Test]
    public void Set_StrategyIgnore_Equal()
    {
        var act = new Student
        {
            Name = "StudentName",
            Age = 1,
            Courses = new[] { new Course { Name = "CourseName" } }
        };

        var exp = new Student
        {
            Name = "StudentName1",
            Age = 1,
            Courses = new[] { new Course { Name = "CourseName1" } }
        };

        var actual = act.DeeplyEquals(exp, propName => propName == "Student.Name");

        var expected =
            DeepEqualityResult.Create(new Distinction("Student.Courses[0].Name", "CourseName", "CourseName1"));
        CollectionAssert.AreEquivalent(expected, actual);
    }

    [Test]
    public void DeeplyEquals_ShouldIgnoreCollectionMemberProperty_WhenIgnoreTokenOmitsIndex()
    {
        var expected = new List<ClassA>
        {
            new() { One = "1" },
            new() { Two = 2 }
        };

        var actual = new List<ClassA>
        {
            new() { One = "2" },
            new() { Two = 2 }
        };

        var result = expected.DeeplyEquals(actual, "One");

        result.Should().BeEmpty();
    }

    [Test]
    public void CollectionComparisonResultComparer_Should_RespectIgnoreStrategy_BeforeNullChecks()
    {
        var a1 = new A
        {
            B = new List<B>
            {
                new() { C = null }
            }
        };

        var a2 = new A
        {
            B = new List<B>
            {
                new() { C = "C" }
            }
        };

        var result = a1.DeeplyEquals(a2, Ignore.IgnoreProperty);
    }
}