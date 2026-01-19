using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies;
using ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties;
using ObjectsComparator.Tests.TestModels;
using System;
using System.Collections.Generic;

namespace ObjectsComparator.Tests;

[TestFixture]
public class StrategyTests
{
    private readonly Time _time = new("2016", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34);

    private readonly DigitalClock _dDigitalClock = new(true, new[] { 1, 2 },
        new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34)), "2015", 1.2F, 11, 1.12,
        new List<string> { "df", "asd" }, 1, 9);

    [Test]
    public void DisplayCustomErrorMsg()
    {
        var actual = new Student
        {
            Name = "Alex",
            Age = 20,
            Vehicle = new Vehicle { Model = "Audi" },
            Courses = new[]
            {
                new Course { Name = "Math", Duration = TimeSpan.FromHours(4) },
                new Course { Name = "Liter", Duration = TimeSpan.FromHours(4) }
            }
        };

        var expected = new Student
        {
            Name = "Bob",
            Age = 20,
            Vehicle = new Vehicle { Model = "Opel" },
            Courses = new[]
            {
                new Course { Name = "Math", Duration = TimeSpan.FromHours(3) },
                new Course { Name = "Literature", Duration = TimeSpan.FromHours(4) }
            }
        };

        var skip = new[] { "Vehicle", "Name", "Courses[1].Name" };
        var result = expected.DeeplyEquals(actual,
            str => str.Set(x => x.Courses[0].Duration, (act, exp) => act > TimeSpan.FromHours(3),
                x => x.SetExpectedInformation("Expected that Duration should be more that 3 hours")), skip);

        result.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new Distinction(
                "Student.Courses[0].Duration",
                "Expected that Duration should be more that 3 hours",
                "04:00:00", "(act:(03:00:00), exp:(04:00:00)) => (act:(03:00:00) > 03:00:00)"));
    }

    [Test]
    public void ComparableDisplayCustomErrorMsg()
    {
        var actual = new StudentNew
        {
            Name = "Alex",
            Age = 20,
            Vehicle = new Vehicle { Model = "Audi" },
            Courses = new[]
            {
                new CourseNew { Name = "Math", Duration = TimeSpan.FromHours(4) },
                new CourseNew { Name = "Liter", Duration = TimeSpan.FromHours(4) }
            }
        };

        var expected = new StudentNew
        {
            Name = "Bob",
            Age = 20,
            Vehicle = new Vehicle { Model = "Opel" },
            Courses = new[]
            {
                new CourseNew { Name = "Math", Duration = TimeSpan.FromHours(3) },
                new CourseNew { Name = "Literature", Duration = TimeSpan.FromHours(4) }
            }
        };

        var skip = new[] { "Vehicle", "Name", "Courses[1].Name" };

        var result = expected.DeeplyEquals(actual,
            str => str.Set(x => x.Courses[0].Duration, (act, exp) => act > TimeSpan.FromHours(3),
                x => x.SetExpectedInformation("Expected that Duration should be more that 3 hours")), skip);

        result.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new Distinction(
                "StudentNew.Courses[0].Duration",
                "Expected that Duration should be more that 3 hours",
                "04:00:00", "(act:(03:00:00), exp:(04:00:00)) => (act:(03:00:00) > 03:00:00)"));
    }

    [Test]
    public void Comparable()
    {
        var actual = new StudentNew
        {
            Name = "Alex",
            Age = 20,
            Vehicle = new Vehicle { Model = "Audi" },
            Courses = new[]
            {
                new CourseNew { Name = "Math", Duration = TimeSpan.FromHours(4) },
                new CourseNew { Name = "Liter", Duration = TimeSpan.FromHours(4) }
            }
        };

        var expected = new StudentNew
        {
            Name = "Alex",
            Age = 20,
            Vehicle = new Vehicle { Model = "Audi" },
            Courses = new[]
            {
                new CourseNew { Name = "Math", Duration = TimeSpan.FromHours(3) },
                new CourseNew { Name = "Liter", Duration = TimeSpan.FromHours(4) }
            }
        };

        var result = expected.DeeplyEquals(actual);

        result.Should()
            .BeEquivalentTo(
                new[]
                {
                    new Distinction("StudentNew.Courses[0].Duration", TimeSpan.FromHours(3),
                        TimeSpan.FromHours(4), "== (Equality Operator)")
                });
    }

    [Test]
    public void IsObjectsEqualTestWhenIgnore()
    {
        var time1 = new Time("wrong", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34);
        var resultNoDiffTime1 = _time.DeeplyEquals(time1, "PropYear");
        resultNoDiffTime1.Should().BeEmpty();
    }

    [Test]
    public void IsObjectsEqualTestWhenIgnoreInnerObject()
    {
        var d2DigitalClock = new DigitalClock(true, new[] { 1, 2 },
            new Calendar(3, new Time("wrong", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34)), "2015", 1.2F, 11,
            1.12,
            new List<string> { "df", "asd" }, 1, 9);
        var resultNoDiffTime1 =
            _dDigitalClock.DeeplyEquals(d2DigitalClock, "PropCalendar.PropTimePanel.PropYear");
        resultNoDiffTime1.Should().BeEmpty();
    }

    [Test]
    public void IsObjectsEqualTestWhenSetStrategyInnerObject()
    {
        const int page = 4;
        var d2DigitalClock = new DigitalClock(true, new[] { 1, 2 },
            new Calendar(4, new Time("2016", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34)), "2015", 1.2F, 11,
            1.12,
            new List<string> { "df", "asd" }, 1, 9);
        var str = new Strategies<DigitalClock>().Set(x => x.PropCalendar.Page,
            (s, s1) => s1 == page);
        var resultNoDiffTime1 = _dDigitalClock.DeeplyEquals(d2DigitalClock, str);
        resultNoDiffTime1.Should().BeEmpty();
    }

    [Test]
    public void IsObjectsEqualTestWhenSeveralIgnore()
    {
        var time1 = new Time("wrong", 1.5F, 77, 1.2, new List<string> { "", "" }, 4, 34);
        var resultNoDiffTime1 = _time.DeeplyEquals(time1, "PropYear", "Day");
        resultNoDiffTime1.Should().BeEmpty();
    }

    [Test]
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

        var res = act.DeeplyEquals(exp, str => str.Set(x => x.InnerClass[0].Foo,
            (s, s1) => s == data));
        res.Should().BeEmpty();
    }

    [Test]
    public void Set_Strategy_For_Collection_Ref_Type_NegativeTest()
    {
        const string data = "actual";
        var act = new ClassA
        {
            One = "f",
            Two = 5,
            ArrayThird = new[] { "test", "test1" },
            InnerClass = new[] { new SomeClass { Foo = data }, new SomeClass { Foo = data } }
        };

        var exp = new ClassA
        {
            One = "f",
            Two = 5,
            ArrayThird = new[] { "error", "error1" },
            InnerClass = new[]
            {
                new SomeClass { Foo = "some" }, new SomeClass { Foo = "someFail" },
                new SomeClass { Foo = "someFail1" }
            }
        };

        var res = act.DeeplyEquals(exp, str => str.Set(x => x.InnerClass[0].Foo,
            (s, s1) => s == data));

        res.Should().BeEquivalentTo(new[]
        {
            new Distinction("ClassA.ArrayThird[0]", "test", "error"),
            new Distinction("ClassA.ArrayThird[1]", "test1", "error1"),
            new Distinction("ClassA.InnerClass[1].Foo", "actual", "someFail"),
            new Distinction("ClassA.InnerClass[2]", null,
                JsonConvert.SerializeObject(new SomeClass { Foo = "someFail1" }, SerializerSettings.Settings),
                "Added")
        });
    }

    [Test]
    public void Set_Strategy_For_Member()
    {
        var act = new ClassA
        {
            One = "f",
            Two = 5,
            ArrayThird = new[] { "sss", "ggg" },
            InnerClass = new[] { new SomeClass { Foo = "some" }, new SomeClass { Foo = "some2" } }
        };

        var exp = new ClassA
        {
            One = "f",
            Two = 77,
            ArrayThird = new[] { "sss", "ggg" },
            InnerClass = new[] { new SomeClass { Foo = "some" }, new SomeClass { Foo = "some2" } }
        };

        var res = act.DeeplyEquals(exp, st => st.Set(x => x.Two,
            (s, s1) => s == 5));
        res.Should().BeEmpty();
    }

    [Test]
    public void Set_Strategy_For_Member_Array()
    {
        var act = new Building
        {
            Address = "NY, First Street",
            ListOfAppNumbers = new[] { 32, 25, 14, 89 }
        };

        var exp = new Building
        {
            Address = "NY, First Street",
            ListOfAppNumbers = new[] { 555, 25, 14, 89 }
        };

        const int expNumber = 32;

        var str = new Strategies<Building>().Set(x => x.ListOfAppNumbers[0],
            (ex, ac) => ex == expNumber);

        var res = act.DeeplyEquals(exp, str);
        res.Should().BeEmpty();
    }

    [Test]
    public void Set_Strategy_For_Member_List()
    {
        var act = new BuildingList
        {
            Address = "NY, First Street",
            ListOfAppNumbers = new[] { 32, 25, 14, 89 }
        };

        var exp = new BuildingList
        {
            Address = "NY, First Street",
            ListOfAppNumbers = new[] { 555, 25, 14, 89 }
        };

        const int expNumber = 32;

        var str = new Strategies<BuildingList>().Set(x => x.ListOfAppNumbers[0],
            (ex, ac) => ex == expNumber);

        var res = act.DeeplyEquals(exp, str);
        res.Should().BeEmpty();
    }

    [Test]
    public void Set_Strategy_For_Member_RefType()
    {
        var act = new ClassB
        {
            One = "f",
            Two = new SomeClass { Foo = "no" },
            Third = new SomeClass { Foo = "yes" }
        };

        var exp = new ClassB
        {
            One = "f",
            Two = new SomeClass { Foo = "no" },
            Third = new SomeClass { Foo = "no" }
        };

        var res = act.DeeplyEquals(exp, str => str.Set(x => x.Third,
            (s, s1) => s.Foo == "yes"));
        res.Should().BeEmpty();
    }

    [Test]
    public void Set_Strategy_For_Null_Member()
    {
        var act = new ClassB
        {
            One = "f",
            Two = new SomeClass { Foo = "no" },
            Third = null
        };

        var exp = new ClassB
        {
            One = "f",
            Two = new SomeClass { Foo = "no" },
            Third = new SomeClass { Foo = "yes" }
        };

        var res = act.DeeplyEquals(exp, str => str.Set(x => x.Third,
            (actual, expected) => expected != null));
        res.Should().BeEmpty();
    }

    [Test]
    public void DistinctionForEqualsOverride()
    {
        var time1 = new StudentEq { Age = 3, Courses = new[] { new CourseE { Name = "fff" } } };
        var time2 = new StudentEq { Age = 3, Courses = new[] { new CourseE { Name = "222" } } };
        var resultNoDiffTime1 = time1.DeeplyEquals(time2);
        resultNoDiffTime1.Should().NotBeEmpty();
        resultNoDiffTime1[0].Details.Should()
            .BeEquivalentTo("Was used override 'Equals()'");
    }

    [Test]
    public void MemberComparison_WhenOverridesEqualsSkipped_ShouldReportNestedPropertyDifferences()
    {
        var expected = new StudentEq { Age = 3, Courses = new[] { new CourseE { Name = "fff" } } };
        var actual = new StudentEq { Age = 3, Courses = new[] { new CourseE { Name = "222" } } };

        var options = ComparatorOptions.SkipStrategies(StrategyType.OverridesEquals);

        var result = expected.DeeplyEquals(actual, options);
        result.Should().BeEquivalentTo(
            new[]
            {
                new Distinction("StudentEq.Courses[0].Name", "fff", "222")
            });
    }

    [Test]
    public void EqualityAndCompareToSkipped_FallsBackToPrimitiveComparison_NoDetails()
    {
        var actual = new CourseNew3 { Name = "Math", Duration = 5 };
        var expected = new CourseNew3 { Name = "Math", Duration = 4 };

        var options = ComparatorOptions.SkipStrategies(StrategyType.Equality, StrategyType.OverridesEquals,
            StrategyType.CompareTo);

        bool equal = expected.DeeplyEquals(actual, options);
        equal.Should().BeFalse();
    }

    [Test]
    public void DeeplyEquals_WithOptionsAndIgnoreTokens_ShouldIgnoreSpecifiedMembers()
    {
        var expected = new Student
        {
            Name = "StudentName",
            Age = 1,
            Courses = new[] { new Course { Name = "CourseName" } }
        };

        var actual = new Student
        {
            Name = "StudentName1",
            Age = 1,
            Courses = new[] { new Course { Name = "CourseName1" } }
        };

        var options = new ComparatorOptions();
        var res = expected.DeeplyEquals(actual, options, "Name", "Courses[0].Name");

        res.Should().BeEmpty();
    }
}