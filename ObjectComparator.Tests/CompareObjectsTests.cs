using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies;
using ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties;
using ObjectsComparator.Tests.TestModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectsComparator.Tests
{
    [TestFixture]
    public class CompareObjectsTests
    {
        private readonly Time _time = new("2016", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34);

        private readonly DigitalClock _dDigitalClock = new(true, new[] { 1, 2 },
            new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34)), "2015", 1.2F, 11, 1.12,
            new List<string> { "df", "asd" }, 1, 9);

        [Test]
        public void DictionaryVerifications()
        {
            var exp = new Library
            {
                Books = new Dictionary<string, Book>
                {
                    ["hobbit"] = new() { Pages = 1000, Text = "hobbit Text" },
                    ["murder in orient express"] = new() { Pages = 500, Text = "murder in orient express Text" },
                    ["Shantaram"] = new() { Pages = 500, Text = "Shantaram Text" }
                }
            };

            var act = new Library
            {
                Books = new Dictionary<string, Book>
                {
                    ["hobbit"] = new() { Pages = 1, Text = "hobbit Text" },
                    ["murder in orient express"] = new() { Pages = 500, Text = "murder in orient express Text1" },
                    ["Shantaram"] = new() { Pages = 500, Text = "Shantaram Text" },
                    ["Shantaram1"] = new() { Pages = 500, Text = "Shantaram Text" }
                }
            };

            var result = exp.DeeplyEquals(act);
            var expectedDistinctionsCollection = DeepEqualityResult.Create(new[]
            {
                new Distinction("Library.Books", null, "Shantaram1", "Added"),
                new Distinction("Library.Books[hobbit].Pages", 1000, 1),
                new Distinction(
                    "Library.Books[murder in orient express].Text", "murder in orient express Text",
                    "murder in orient express Text1")
            });

            CollectionAssert.AreEquivalent(result, expectedDistinctionsCollection);
        }

        [Test]
        public void DictionaryVerifications_Different_length()
        {
            var exp = new Library3
            {
                Books = new Dictionary<SomeKey, Book>
                {
                    [new SomeKey("hobbit")] = new() { Pages = 1000, Text = "hobbit Text" },
                    [new SomeKey("murder in orient express")] =
                        new() { Pages = 500, Text = "murder in orient express Text" },
                    [new SomeKey("Shantaram")] = new() { Pages = 500, Text = "Shantaram Text" }
                }
            };

            var act = new Library3
            {
                Books = new Dictionary<SomeKey, Book>
                {
                    [new SomeKey("hobbit")] = new() { Pages = 1, Text = "hobbit Text" },
                }
            };

            var result = exp.DeeplyEquals(act);
            var expectedDistinctionsCollection = DeepEqualityResult.Create(new[]
            {
                new Distinction("Library3.Books",
                    $"{new SomeKey("murder in orient express")}, {new SomeKey("Shantaram")}", null, "Removed"),
                new Distinction("Library3.Books[SomeKey { Key = hobbit }].Pages", 1000, 1)
            });

            CollectionAssert.AreEquivalent(result, expectedDistinctionsCollection);
        }

        [Test]
        public void DisplayCustomErrorMsg()
        {
            var actual = new Student
            {
                Name = "Alex",
                Age = 20,
                Vehicle = new Vehicle
                {
                    Model = "Audi"
                },
                Courses = new[]
                {
                    new Course
                    {
                        Name = "Math",
                        Duration = TimeSpan.FromHours(4)
                    },
                    new Course
                    {
                        Name = "Liter",
                        Duration = TimeSpan.FromHours(4)
                    }
                }
            };

            var expected = new Student
            {
                Name = "Bob",
                Age = 20,
                Vehicle = new Vehicle
                {
                    Model = "Opel"
                },
                Courses = new[]
                {
                    new Course
                    {
                        Name = "Math",
                        Duration = TimeSpan.FromHours(3)
                    },
                    new Course
                    {
                        Name = "Literature",
                        Duration = TimeSpan.FromHours(4)
                    }
                }
            };

            var skip = new[] { "Vehicle", "Name", "Courses[1].Name" };
            var result = expected.DeeplyEquals(actual,
                str => str.Set(x => x.Courses[0].Duration, (act, exp) => act > TimeSpan.FromHours(3),
                    x => x.SetExpectedInformation("Expected that Duration should be more that 3 hours")), skip);

            var expectedDistinctionsCollection = DeepEqualityResult.Create(new Distinction(
                "Student.Courses[0].Duration",
                "Expected that Duration should be more that 3 hours",
                "04:00:00", "(act:(03:00:00), exp:(04:00:00)) => (act:(03:00:00) > 03:00:00)"));


            CollectionAssert.AreEquivalent(result, expectedDistinctionsCollection);
        }

        [Test]
        public void ComparableDisplayCustomErrorMsg()
        {
            var actual = new StudentNew
            {
                Name = "Alex",
                Age = 20,
                Vehicle = new Vehicle
                {
                    Model = "Audi"
                },
                Courses = new[]
                {
                    new CourseNew
                    {
                        Name = "Math",
                        Duration = TimeSpan.FromHours(4)
                    },
                    new CourseNew
                    {
                        Name = "Liter",
                        Duration = TimeSpan.FromHours(4)
                    }
                }
            };

            var expected = new StudentNew
            {
                Name = "Bob",
                Age = 20,
                Vehicle = new Vehicle
                {
                    Model = "Opel"
                },
                Courses = new[]
                {
                    new CourseNew
                    {
                        Name = "Math",
                        Duration = TimeSpan.FromHours(3)
                    },
                    new CourseNew
                    {
                        Name = "Literature",
                        Duration = TimeSpan.FromHours(4)
                    }
                }
            };

            var skip = new[] { "Vehicle", "Name", "Courses[1].Name" };

            var result = expected.DeeplyEquals(actual,
                str => str.Set(x => x.Courses[0].Duration, (act, exp) => act > TimeSpan.FromHours(3),
                    x => x.SetExpectedInformation("Expected that Duration should be more that 3 hours")), skip);

            var expectedDistinctionsCollection = DeepEqualityResult.Create(new Distinction(
                "StudentNew.Courses[0].Duration",
                "Expected that Duration should be more that 3 hours",
                "04:00:00", "(act:(03:00:00), exp:(04:00:00)) => (act:(03:00:00) > 03:00:00)"));

            CollectionAssert.AreEquivalent(result, expectedDistinctionsCollection);
        }

        [Test]
        public void Comparable()
        {

            var actual = new StudentNew
            {
                Name = "Alex",
                Age = 20,
                Vehicle = new Vehicle
                {
                    Model = "Audi"
                },
                Courses = new[]
                {
                    new CourseNew
                    {
                        Name = "Math",
                        Duration = TimeSpan.FromHours(4)
                    },
                    new CourseNew
                    {
                        Name = "Liter",
                        Duration = TimeSpan.FromHours(4)
                    }
                }
            };

            var expected = new StudentNew
            {
                Name = "Alex",
                Age = 20,
                Vehicle = new Vehicle
                {
                    Model = "Audi"
                },
                Courses = new[]
                {
                    new CourseNew
                    {
                        Name = "Math",
                        Duration = TimeSpan.FromHours(3)
                    },
                    new CourseNew
                    {
                        Name = "Liter",
                        Duration = TimeSpan.FromHours(4)
                    }
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
        public void DistinctionExist()
        {
            var time1 = new Time("wrong", 1.5F, 3, 1.2, new List<string> { "", "" }, 4, 34);
            var resultNoDiffTime1 = _time.DeeplyEquals(time1);
            resultNoDiffTime1.Should().NotBeEmpty();
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
        public void IsObjectsFallTest_Time_ShortDiffTime()
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

            var expected = DeepEqualityResult.Create(new[]
            {
                new Distinction("ClassA.ArrayThird[0]", "test", "error"),
                new Distinction("ClassA.ArrayThird[1]", "test1", "error1"),
                new Distinction("ClassA.InnerClass[1].Foo", "actual", "someFail"),
                new Distinction("ClassA.InnerClass[2]", null,
                    JsonConvert.SerializeObject(new SomeClass { Foo = "someFail1" }, SerializerSettings.Settings),
                    "Added")
            });

            CollectionAssert.AreEquivalent(res, expected);
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
        public void Set_StrategyIgnore()
        {
            var act = new Student
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

            var exp = new Student
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
                Courses = new[]
                {
                    new Course
                    {
                        Name = "CourseName"
                    }
                }
            };

            var exp = new Student
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

            var actual = act.DeeplyEquals(exp, propName => propName == "Student.Name");

            var expected =
                DeepEqualityResult.Create(new Distinction("Student.Courses[0].Name", "CourseName", "CourseName1"));
            CollectionAssert.AreEquivalent(expected, actual);
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

            var expected =
                DeepEqualityResult.Create(new Distinction("StudentIName.Courses[0].Name", "CourseName", "CourseName1"));
            CollectionAssert.AreEquivalent(expected, actual);
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
        public void Anonymous_Types() => new { Integer = 1, String = "Test", Nested = new byte[] { 1, 2, 3 } }
            .DeeplyEquals(new { Integer = 1, String = "Test", Nested = new byte[] { 1, 2, 4 } }).Any().Should()
            .BeTrue();

        [Test]
        public void AreDeeplyEqualShouldReportCorrectlyWithDictionaries()
        {
            var firstDictionary = new Dictionary<string, string>
            {
                { "Key", "Value" },
                { "AnotherKey", "Value" },
            };

            var secondDictionary = new Dictionary<string, string>
            {
                { "Key", "Value" },
                { "AnotherKey", "AnotherValue" },
            };

            firstDictionary.DeeplyEquals(secondDictionary)[0].Should()
                .Be(new Distinction("Dictionary<String, String>[AnotherKey]", "Value", "AnotherValue"));
        }

        [Test]
        public void CompareIDictionaryProperty()
        {
            var exp = new Library2
            {
                Books = new Dictionary<string, Book>
                {
                    ["hobbit"] = new() { Pages = 1000, Text = "hobbit Text" },
                }
            };

            var act = new Library2
            {
                Books = new Dictionary<string, Book>
                {
                    ["hobbit"] = new() { Pages = 1, Text = "hobbit Text" },
                }
            };

            var result = exp.DeeplyEquals(act);
            var expectedDistinctionsCollection = DeepEqualityResult.Create(new[]
            {
                new Distinction("Library2.Books[hobbit].Pages", 1000, 1),
            });

            CollectionAssert.AreEquivalent(result, expectedDistinctionsCollection);
        }

        [Test]
        public void CompareIDictionaryImplementation()
        {
            var firstDictionary = new StringDictionary
            {
                { "Key", "Value" },
                { "AnotherKey", "Value" },
            };

            var secondDictionary = new StringDictionary
            {
                { "Key", "Value" },
                { "AnotherKey", "AnotherValue" },
            };

            firstDictionary.DeeplyEquals(secondDictionary)[0].Should()
                .Be(new Distinction("StringDictionary[AnotherKey]", "Value", "AnotherValue"));
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
        public void DeeplyEquals_ShouldReturnJoinedCollectionValuesDifference_WhenCollectionLengthsDiffer()
        {
            var actual = new GroupPortals
            {
                Portals = new List<int> { 1, 2, 3, 5 },
                Portals1 = new List<GroupPortals1>
                {
                    new GroupPortals1
                    {
                        Courses = new List<Course>
                        {
                            new Course
                            {
                                Name = "test"
                            }
                        }
                    }
                }
            };

            var expected = new GroupPortals
            {
                Portals = new List<int> { 1, 2, 3, 4, 7, 0 },
                Portals1 = new List<GroupPortals1>
                {
                    new GroupPortals1
                    {
                        Courses = new List<Course>
                        {
                            new Course
                            {
                                Name = "test1"
                            }
                        }
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
        public void CompareIEnumerableImplementationAsObject()
        {
            object act = new StringList { "A", "B" };
            object exp = new StringList { "B", "C" };

            exp.DeeplyEquals(act)[0].Should()
                .Be(new Distinction("StringList[0]", "B", "A"));
        }

        [Test]
        public void CollectionComparisonResultComparer_Should_RespectIgnoreStrategy_BeforeNullChecks()
        {
            var a1 = new A
            {
                B = new List<B>
                {
                    new B
                    {
                        C = null
                    }
                }
            };
            var a2 = new A
            {
                B = new List<B>
                {
                    new B
                    {
                        C = "C"
                    }
                }
            };
            var result = a1.DeeplyEquals(a2, Ignore.IgnoreProperty);
        }

        [Test]
        public void CompareIDictionaryImplementationAsObject()
        {
            object firstDictionary = new StringDictionary
            {
                { "Key", "Value" },
                { "AnotherKey", "Value" },
            };

            object secondDictionary = new StringDictionary
            {
                { "Key", "Value" },
                { "AnotherKey", "AnotherValue" },
            };

            firstDictionary.DeeplyEquals(secondDictionary)[0].Should()
                .Be(new Distinction("StringDictionary[AnotherKey]", "Value", "AnotherValue"));
        }

        [Test]
        public void ToJson_ShouldSerializeDistinctionsCollectionCorrectly()
        {
            // Arrange
            var distinctions = DeepEqualityResult.Create(new[]
            {
                new Distinction("Snapshot.Rules[2].Expression", "Amount > 100", "Amount > 200"),
                new Distinction("Snapshot.Rules[6].Name", "OldName", "NewName"),
                new Distinction("Snapshot.Portals", null, 91, "Added"),
                new Distinction("Snapshot.Portals", null, 101, "Added"),
                new Distinction("Snapshot.Portals", 1000, null, "Removed"),
                new Distinction("Snapshot.Portals[0].Title", "Main Portal", "Main Portal v2"),
            });

            var actualJson = DeepEqualsExtension.ToJson(distinctions);

            // Expected JSON
            var expectedJson = """
                               {
                                 "Rules": {
                                   "2": {
                                     "Expression": {
                                       "before": "Amount > 100",
                                       "after": "Amount > 200",
                                       "details": ""
                                     }
                                   },
                                   "6": {
                                     "Name": {
                                       "before": "OldName",
                                       "after": "NewName",
                                       "details": ""
                                     }
                                   }
                                 },
                                 "Portals": {
                                   "Added": {
                                     "before": null,
                                     "after": 101,
                                     "details": "Added"
                                   },
                                   "Removed": {
                                     "before": 1000,
                                     "after": null,
                                     "details": "Removed"
                                   },
                                   "0": {
                                     "Title": {
                                       "before": "Main Portal",
                                       "after": "Main Portal v2",
                                       "details": ""
                                     }
                                   }
                                 }
                               }
                               """;

            // Assert
            var normalizedExpected = JObject.Parse(expectedJson).ToString();
            var normalizedActual = JObject.Parse(actualJson).ToString();

            Assert.AreEqual(normalizedExpected, normalizedActual);
        }

        [Test]
        public void MemberComparison_WhenOverridesEqualsSkipped_ShouldReportNestedPropertyDifferences()
        {
            var expected = new StudentEq { Age = 3, Courses = new[] { new CourseE { Name = "fff" } } };
            var actual = new StudentEq { Age = 3, Courses = new[] { new CourseE { Name = "222" } } };

            var options = new ComparatorOptions();
            options.StrategyTypeSkipList.Add(StrategyType.OverridesEquals);

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

            var options = ComparatorOptions.Create(StrategyType.Equality, StrategyType.OverridesEquals,
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
                Courses = new[]
                {
                    new Course { Name = "CourseName" }
                }
            };

            var actual = new Student
            {
                Name = "StudentName1",
                Age = 1,
                Courses = new[]
                {
                    new Course { Name = "CourseName1" }
                }
            };

            var options = new ComparatorOptions();
            // Ignore Name everywhere via tokens (converted to full path by DeepEqualsExtension)
            var res = expected.DeeplyEquals(actual, options, "Name", "Courses[0].Name");

            res.Should().BeEmpty();
        }
    }
}