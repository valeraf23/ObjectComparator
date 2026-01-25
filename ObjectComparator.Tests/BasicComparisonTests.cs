using FluentAssertions;
using NUnit.Framework;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Tests.TestModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectsComparator.Tests;

[TestFixture]
public class BasicComparisonTests
{
    private readonly Time _time = new("2016", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34);

    private readonly DigitalClock _dDigitalClock = new(true, new[] { 1, 2 },
        new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34)), "2015", 1.2F, 11, 1.12,
        new List<string> { "df", "asd" }, 1, 9);

    [Test]
    public void DistinctionExist()
    {
        var time1 = new Time("wrong", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34);
        var resultNoDiffTime1 = _time.DeeplyEquals(time1);
        resultNoDiffTime1.Should().NotBeEmpty();
    }

    [Test]
    public void IsObjects_NullableNotEqual()
    {
        var act = new ClassA
        {
            One = "f",
            Two = null,
            ArrayThird = new[] { "sss", "ggg" },
            InnerClass = new[] { new SomeClass { Foo = "ttt" }, new SomeClass { Foo = "ttt2" } }
        };

        var exp = new ClassA
        {
            One = "f",
            Two = 5,
            ArrayThird = new[] { "sss", "ggg" },
            InnerClass = new[] { new SomeClass { Foo = "ttt" }, new SomeClass { Foo = "ttt2" } }
        };

        var res = act.DeeplyEquals(exp);
        ((bool)res).Should().BeFalse();
        var diff = res.First();
        diff.ActualValue.Should().Be(exp.Two);
        diff.ExpectedValue.Should().Be("null");
    }

    [Test]
    public void IsObjects_Several_Diff()
    {
        var act = new ClassA
        {
            One = "f",
            Two = null,
            ArrayThird = new[] { "sss", "ggg" },
            InnerClass = new[] { new SomeClass { Foo = "ttt" }, new SomeClass { Foo = "ttt2" } }
        };

        var exp = new ClassA
        {
            One = "f",
            Two = 5,
            ArrayThird = new[] { "sss", "ggg" },
            InnerClass = new[] { new SomeClass { Foo = "ttt" }, new SomeClass { Foo = "tt" } }
        };

        var res = act.DeeplyEquals(exp);
        res.Count().Should().Be(2);
        var diff = res.First();
        diff.Path.Should().Be($"{nameof(ClassA)}.{nameof(act.Two)}");
        diff.ActualValue.Should().Be(exp.Two);
        diff.ExpectedValue.Should().Be("null");
        var diff1 = res[1];
        diff1.Path.Should().Be($"{nameof(ClassA)}.{nameof(act.InnerClass) + "[1]" + ".Foo"}");
        diff1.ActualValue.Should().Be(exp.InnerClass[1].Foo);
        diff1.ExpectedValue.Should().Be(act.InnerClass[1].Foo);
    }

    [Test]
    public void IsObjectsEqualTest()
    {
        var time1 = new Time("2016", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34);
        bool resultNoDiffTime1 = _time.DeeplyEquals(time1);
        resultNoDiffTime1.Should().BeTrue();
    }

    [Test]
    public void IsObjectsEqualTest_AllDigitalClock()
    {
        var d1DigitalClock = new DigitalClock(true, new[] { 1, 2 },
            new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34)), "2015", 1.2F, 11,
            1.12,
            new List<string> { "df", "asd" }, 1, 9);
        var resultNoDiffClock = _dDigitalClock.DeeplyEquals(d1DigitalClock);
        resultNoDiffClock.Should().BeEmpty();
    }

    [Test]
    public void IsObjectsFallTest_DigitalClock_ArrayDiff()
    {
        const int act = 11;
        var d3DigitalClock = new DigitalClock(true, new[] { act, 2 },
            new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34)), "2015", 1.2F, 11,
            1.12,
            new List<string> { "df", "asd" }, 1, 9);
        var resultArrayDiffClock = _dDigitalClock.DeeplyEquals(d3DigitalClock);
        resultArrayDiffClock.Should().NotBeEmpty();
        var diff = resultArrayDiffClock.First();
        diff.Path.Should().BeEquivalentTo($"{nameof(DigitalClock)}.{nameof(d3DigitalClock.NumberMonth)}[0]");
        diff.ActualValue.Should().Be(act);
        diff.ExpectedValue.Should().Be(_dDigitalClock.NumberMonth[0]);
    }

    [Test]
    public void IsObjectsFallTest_DigitalClock_BoolDiff()
    {
        const bool act = false;
        var d2DigitalClock = new DigitalClock(act, new[] { 1, 2 },
            new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34)), "2015", 1.2F, 11,
            1.12,
            new List<string> { "df", "asd" }, 1, 9);
        var resultBoolDiffClock = _dDigitalClock.DeeplyEquals(d2DigitalClock);
        resultBoolDiffClock.Should().NotBeEmpty();
        var diff = resultBoolDiffClock.First();
        diff.ActualValue.Should().Be(act);
        diff.ExpectedValue.Should().Be(_dDigitalClock.ClickTimer);
    }

    [Test]
    public void IsObjectsFallTest_Time_FloatDiff()
    {
        const float actual = 2.5F;
        var time3 = new Time("2016", actual, 3, 1.2, new List<string> { "", "" }, 4, 34);
        var resultFloatDiffTime1 = _time.DeeplyEquals(time3);
        resultFloatDiffTime1.Should().NotBeEmpty();
        var diff = resultFloatDiffTime1.First();
        diff.ActualValue.Should().Be(actual);
        diff.ExpectedValue.Should().Be(_time.Seconds);
    }

    [Test]
    public void IsObjectsFallTest_Time_ListDiffTime()
    {
        const string actual = "ddd";
        var time6 = new Time("2016", 1.5F, 3, 1.2, new List<string> { "ddd", "" }, 4, 34);
        var resultCollectionDiffTime = _time.DeeplyEquals(time6);
        resultCollectionDiffTime.Should().NotBeEmpty();
        var diff = resultCollectionDiffTime.First();
        diff.ActualValue.Should().Be(actual);
        diff.ExpectedValue.Should().Be(_time.Week[0]);
    }

    [Test]
    public void IsObjectsFallTest_Time_ShortDiff()
    {
        const short actual = 1;
        var time4 = new Time("2016", 1.5F, actual, 1.2, new List<string> { "", "" }, 4, 34);
        var resultShortDiffTime = _time.DeeplyEquals(time4);
        resultShortDiffTime.Should().NotBeEmpty();
        var diff = resultShortDiffTime.First();
        diff.ActualValue.Should().Be(actual);
        diff.ExpectedValue.Should().Be(_time.Day);
    }

    [Test]
    public void IsObjectsFallTest_Time_StringDiff()
    {
        const string actual = "2015";
        var time2 = new Time(actual, 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34);
        var resultStringDiffTime1 = _time.DeeplyEquals(time2);
        resultStringDiffTime1.Should().NotBeEmpty();
        var diff = resultStringDiffTime1.First();
        diff.Path.Should().BeEquivalentTo($"{nameof(Time)}.{nameof(time2.PropYear)}");
        diff.ActualValue.Should().Be(actual);
        diff.ExpectedValue.Should().Be(_time.PropYear);
    }

    [Test]
    public void ProperlyNamePropertiesForInnerObjects()
    {
        var d2DigitalClock = new DigitalClock(true, new[] { 1, 2 },
            new Calendar(3, new Time("wrong", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34)), "2015", 1.2F, 11,
            1.12,
            new List<string> { "df", "asd" }, 1, 9);
        var resultNoDiffTime1 = _dDigitalClock.DeeplyEquals(d2DigitalClock);
        resultNoDiffTime1.Should().NotBeEmpty().And.ContainSingle(x =>
            x.Path ==
            $"{nameof(DigitalClock)}.{nameof(d2DigitalClock.PropCalendar)}.{nameof(d2DigitalClock.PropCalendar.PropTimePanel)}.{nameof(d2DigitalClock.PropCalendar.PropTimePanel.PropYear)}");
    }

    [Test]
    public void Nulls()
    {
        StudentNew2 exp = null;

        var act = new StudentNew2
        {
            Name = new CourseNew2
            {
                Duration = TimeSpan.FromHours(4),
                Name = "Test1"
            }
        };

        exp.DeeplyEquals(act).Any().Should().BeTrue();
    }

    [Test]
    public void Equality()
    {
        var exp = new StudentNew2
        {
            Name = new CourseNew2
            {
                Duration = TimeSpan.FromHours(4),
                Name = "Test1"
            }
        };

        var act = new StudentNew2
        {
            Name = new CourseNew2
            {
                Duration = TimeSpan.FromHours(4),
                Name = "Test1"
            }
        };

        exp.DeeplyEquals(act).Any().Should().BeFalse();
    }

    [Test]
    public void Anonymous_Types()
    {
        new { Integer = 1, String = "Test", Nested = new byte[] { 1, 2, 3 } }
            .DeeplyEquals(new { Integer = 1, String = "Test", Nested = new byte[] { 1, 2, 4 } }).Any().Should()
            .BeTrue();
    }

    [Test]
    public void CompareStructs()
    {
        var exp = new TestStruct
        {
            Integer = 5,
            Text = "Test",
            StudentIName = new StudentIName
            {
                Name = "StudentName1",
                Age = 1,
                Courses = new[]
                {
                    new Course
                    {
                        Name = "CourseName1"
                    }
                }
            },
            TestEnum = TestEnum.First
        };

        var act = new TestStruct
        {
            Integer = 5,
            Text = "Test",
            StudentIName = new StudentIName
            {
                Name = "StudentName1",
                Age = 1,
                Courses = new[]
                {
                    new Course
                    {
                        Name = "CourseName"
                    }
                }
            },
            TestEnum = TestEnum.Second
        };

        var actual = act.DeeplyEquals(exp);
        actual.Count().Should().Be(2);
    }

    [Test]
    public void CompareInterfaceType()
    {
        IName act = new StudentIName
        {
            Name = "StudentName",
            Age = 1,
            Courses = new[]
            {
                new Course
                {
                    Name = "CourseName"
                }
            }
        };

        IName exp = new StudentIName
        {
            Name = "StudentName1",
            Age = 1,
            Courses = new[]
            {
                new Course
                {
                    Name = "CourseName1"
                }
            }
        };

        var actual = act.DeeplyEquals(exp, propName => propName == "StudentIName.Name");

        actual.Should().ContainSingle()
            .Which.Should()
            .BeEquivalentTo(new Distinction("StudentIName.Courses[0].Name", "CourseName", "CourseName1"));
    }
}