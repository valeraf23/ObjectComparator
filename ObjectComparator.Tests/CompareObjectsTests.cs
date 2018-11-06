using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectsComparator.Comparator;
using ObjectsComparator.Comparator.StrategiesForCertainProperties;
using ObjectsComparator.Tests.TestModels;

namespace ObjectsComparator.Tests
{
    [TestFixture]
    public class CompareObjectsTests
    {
        private readonly Time _time = new Time("2016", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34);

        private readonly DigitalClock _dDigitalClock = new DigitalClock(true, new[] {1, 2},
            new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34)), "2015", 1.2F, 11, 1.12,
            new List<string> {"df", "asd"}, 1, 9);

        [Test]
        public void IsObjectsEqualTest()
        {
            var time1 = new Time("2016", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34);
            var resultNoDiffTime1 = _time.GetDifferenceBetweenObjects(time1);
            resultNoDiffTime1.Should().BeEmpty();
        }

        [Test]
        public void IsObjectsEqualTestWhenIgnore()
        {
            var time1 = new Time("wrong", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34);
            var resultNoDiffTime1 = _time.GetDifferenceBetweenObjects(time1, "PropYear");
            resultNoDiffTime1.Should().BeEmpty();
        }

        [Test]
        public void DistinctionExist()
        {
            var time1 = new Time("wrong", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34);
            var resultNoDiffTime1 = _time.GetDifferenceBetweenObjects(time1);
            resultNoDiffTime1.Should().NotBeEmpty();
        }

        [Test]
        public void ProperlyNamePropertiesForInnerObjects()
        {
            var d2DigitalClock = new DigitalClock(true, new[] {1, 2},
                new Calendar(3, new Time("wrong", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34)), "2015", 1.2F, 11,
                1.12,
                new List<string> {"df", "asd"}, 1, 9);
            var resultNoDiffTime1 = _dDigitalClock.GetDifferenceBetweenObjects(d2DigitalClock);
            resultNoDiffTime1.Should().NotBeEmpty().And.ContainSingle(x =>
                x.Name ==
                $"{nameof(d2DigitalClock.PropCalendar)}.{nameof(d2DigitalClock.PropCalendar.PropTimePanel)}.{nameof(d2DigitalClock.PropCalendar.PropTimePanel.PropYear)}");
        }

        [Test]
        public void IsObjectsEqualTestWhenIgnoreInnerObject()
        {
            var d2DigitalClock = new DigitalClock(true, new[] {1, 2},
                new Calendar(3, new Time("wrong", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34)), "2015", 1.2F, 11,
                1.12,
                new List<string> {"df", "asd"}, 1, 9);
            var resultNoDiffTime1 =
                _dDigitalClock.GetDifferenceBetweenObjects(d2DigitalClock, "PropCalendar.PropTimePanel.PropYear");
            resultNoDiffTime1.Should().BeEmpty();
        }

        [Test]
        public void IsObjectsEqualTestWhenSetStrategyInnerObject()
        {
            const int page = 4;
            var d2DigitalClock = new DigitalClock(true, new[] {1, 2},
                new Calendar(4, new Time("2016", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34)), "2015", 1.2F, 11,
                1.12,
                new List<string> {"df", "asd"}, 1, 9);
            var str = new Strategies<DigitalClock>().Set(x => x.PropCalendar.Page,
                (s, s1) => s1 == page);
            var resultNoDiffTime1 = _dDigitalClock.GetDifferenceBetweenObjects(d2DigitalClock, str);
            resultNoDiffTime1.Should().BeEmpty();
        }

        [Test]
        public void IsObjectsEqualTestWhenSeveralIgnore()
        {
            var time1 = new Time("wrong", 1.5F, 77, 1.2, new List<string> {"", ""}, 4, 34);
            var resultNoDiffTime1 = _time.GetDifferenceBetweenObjects(time1, "PropYear", "Day");
            resultNoDiffTime1.Should().BeEmpty();
        }

        [Test]
        public void IsObjectsFallTest_Time_StringDiff()
        {
            const string actual = "2015";
            var time2 = new Time(actual, 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34);
            var resultStringDiffTime1 = _time.GetDifferenceBetweenObjects(time2);
            resultStringDiffTime1.Should().NotBeEmpty();
            var diff = resultStringDiffTime1.First();
            diff.Name.Should().BeEquivalentTo(nameof(time2.PropYear));
            diff.ActuallyValue.Should().Be(actual);
            diff.ExpectedValue.Should().Be(_time.PropYear);
        }

        [Test]
        public void IsObjectsFallTest_Time_FloatDiff()
        {
            const float actual = 2.5F;
            var time3 = new Time("2016", actual, 3, 1.2, new List<string> {"", ""}, 4, 34);
            var resultFloatDiffTime1 = _time.GetDifferenceBetweenObjects(time3);
            resultFloatDiffTime1.Should().NotBeEmpty();
            var diff = resultFloatDiffTime1.First();
            diff.ActuallyValue.Should().Be(actual);
            diff.ExpectedValue.Should().Be(_time.Seconds);
        }

        [Test]
        public void IsObjectsFallTest_Time_ShortDiffTime()
        {
            const short actual = 1;
            var time4 = new Time("2016", 1.5F, actual, 1.2, new List<string> {"", ""}, 4, 34);
            var resultShortDiffTime = _time.GetDifferenceBetweenObjects(time4);
            resultShortDiffTime.Should().NotBeEmpty();
            var diff = resultShortDiffTime.First();
            diff.ActuallyValue.Should().Be(actual);
            diff.ExpectedValue.Should().Be(_time.Day);
        }

        [Test]
        public void IsObjectsFallTest_Time_ListDiffTime()
        {
            const string actual = "ddd";
            var time6 = new Time("2016", 1.5F, 3, 1.2, new List<string> {"ddd", ""}, 4, 34);
            var resultCollectionDiffTime = _time.GetDifferenceBetweenObjects(time6);
            resultCollectionDiffTime.Should().NotBeEmpty();
            var diff = resultCollectionDiffTime.First();
            diff.ActuallyValue.Should().Be(actual);
            diff.ExpectedValue.Should().Be(_time.Week[0]);
        }

        [Test]
        public void IsObjectsEqualTest_AllDigitalClock()
        {
            var d1DigitalClock = new DigitalClock(true, new[] {1, 2},
                new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34)), "2015", 1.2F, 11,
                1.12,
                new List<string> {"df", "asd"}, 1, 9);
            var resultNoDiffClock = _dDigitalClock.GetDifferenceBetweenObjects(d1DigitalClock);
            resultNoDiffClock.Should().BeEmpty();
        }

        [Test]
        public void IsObjectsFallTest_DigitalClock_BoolDiff()
        {
            const bool act = false;
            var d2DigitalClock = new DigitalClock(act, new[] {1, 2},
                new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34)), "2015", 1.2F, 11,
                1.12,
                new List<string> {"df", "asd"}, 1, 9);
            var resultBoolDiffClock = _dDigitalClock.GetDifferenceBetweenObjects(d2DigitalClock);
            resultBoolDiffClock.Should().NotBeEmpty();
            var diff = resultBoolDiffClock.First();
            diff.ActuallyValue.Should().Be(act);
            diff.ExpectedValue.Should().Be(_dDigitalClock.ClickTimer);
        }

        [Test]
        public void IsObjectsFallTest_DigitalClock_ArrayDiff()
        {
            const int act = 11;
            var d3DigitalClock = new DigitalClock(true, new[] {act, 2},
                new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34)), "2015", 1.2F, 11,
                1.12,
                new List<string> {"df", "asd"}, 1, 9);
            var resultArrayDiffClock = _dDigitalClock.GetDifferenceBetweenObjects(d3DigitalClock);
            resultArrayDiffClock.Should().NotBeEmpty();
            var diff = resultArrayDiffClock.First();
            diff.Name.Should().BeEquivalentTo(nameof(d3DigitalClock.NumberMonth) + "[0]");
            diff.ActuallyValue.Should().Be(act);
            diff.ExpectedValue.Should().Be(_dDigitalClock.NumberMonth[0]);
        }

        [Test]
        public void IsObjects_NullableNotEqual()
        {
            var act = new ClassA
            {
                One = "f",
                Two = null,
                ArrayThird = new[] {"sss", "ggg"},
                InnerClass = new[] {new SomeClass {Foo = "ttt"}, new SomeClass {Foo = "ttt2"}}
            };

            var exp = new ClassA
            {
                One = "f",
                Two = 5,
                ArrayThird = new[] {"sss", "ggg"},
                InnerClass = new[] {new SomeClass {Foo = "ttt"}, new SomeClass {Foo = "ttt2"}}
            };

            var res = act.GetDifferenceBetweenObjects(exp);
            res.Should().NotBeEmpty();
            var diff = res.First();
            diff.ActuallyValue.Should().Be(exp.Two);
            diff.ExpectedValue.Should().Be("null");
        }

        [Test]
        public void IsObjects_Several_Diff()
        {
            var act = new ClassA
            {
                One = "f",
                Two = null,
                ArrayThird = new[] {"sss", "ggg"},
                InnerClass = new[] {new SomeClass {Foo = "ttt"}, new SomeClass {Foo = "ttt2"}}
            };

            var exp = new ClassA
            {
                One = "f",
                Two = 5,
                ArrayThird = new[] {"sss", "ggg"},
                InnerClass = new[] {new SomeClass {Foo = "ttt"}, new SomeClass {Foo = "tt"}}
            };

            var res = act.GetDifferenceBetweenObjects(exp);
            res.Count().Should().Be(2);
            var diff = res.First();
            diff.Name.Should().Be(nameof(act.Two));
            diff.ActuallyValue.Should().Be(exp.Two);
            diff.ExpectedValue.Should().Be("null");
            var diff1 = res[1];
            diff1.Name.Should().Be(nameof(act.InnerClass) + "[1]" + ".Foo");
            diff1.ActuallyValue.Should().Be(exp.InnerClass[1].Foo);
            diff1.ExpectedValue.Should().Be(act.InnerClass[1].Foo);
        }

        [Test]
        public void Set_Strategy_For_Collection_Ref_Type()
        {
            const string data = "actual";
            var act = new ClassA
            {
                One = "f",
                Two = 5,
                ArrayThird = new[] {"sss", "ggg"},
                InnerClass = new[] {new SomeClass {Foo = data}, new SomeClass {Foo = data}}
            };

            var exp = new ClassA
            {
                One = "f",
                Two = 5,
                ArrayThird = new[] {"sss", "ggg"},
                InnerClass = new[] {new SomeClass {Foo = "some"}, new SomeClass {Foo = data}}
            };

            var res = act.GetDifferenceBetweenObjects(exp, str => str.Set(x => x.InnerClass[0].Foo,
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
                ArrayThird = new[] {"sss", "ggg"},
                InnerClass = new[] {new SomeClass {Foo = data}, new SomeClass {Foo = data}}
            };

            var exp = new ClassA
            {
                One = "f",
                Two = 5,
                ArrayThird = new[] {"error", "error1"},
                InnerClass = new[] {new SomeClass {Foo = "some"}, new SomeClass {Foo = "someFail"}}
            };

            var res = act.GetDifferenceBetweenObjects(exp, str => str.Set(x => x.InnerClass[0].Foo,
                (s, s1) => s == data));

            var expected = new DistinctionsCollection()
                .Add(new Distinction("ArrayThird[0]", "sss", "error"))
                .Add(new Distinction("ArrayThird[1]", "ggg", "error1"))
                .Add(new Distinction("InnerClass[1].Foo", "actual", "someFail"));


            CollectionAssert.AreEquivalent(res, expected);
        }

        [Test]
        public void Set_Strategy_For_Member()
        {

            var act = new ClassA
            {
                One = "f",
                Two = 5,
                ArrayThird = new[] {"sss", "ggg"},
                InnerClass = new[] {new SomeClass {Foo = "some"}, new SomeClass {Foo = "some2"}}
            };

            var exp = new ClassA
            {
                One = "f",
                Two = 77,
                ArrayThird = new[] {"sss", "ggg"},
                InnerClass = new[] {new SomeClass {Foo = "some"}, new SomeClass {Foo = "some2"}}
            };

            var res = act.GetDifferenceBetweenObjects(exp, st => st.Set(x => x.Two,
                (s, s1) => s == 5));
            res.Should().BeEmpty();
        }

        [Test]
        public void Set_Strategy_For_Member_RefType()
        {
            var act = new ClassB
            {
                One = "f",
                Two = new SomeClass {Foo = "no"},
                Third = new SomeClass {Foo = "yes"}

            };

            var exp = new ClassB
            {
                One = "f",
                Two = new SomeClass {Foo = "no"},
                Third = new SomeClass {Foo = "no"}

            };

            var res = act.GetDifferenceBetweenObjects(exp, str => str.Set(x => x.Third,
                (s, s1) => s.Foo == "yes"));
            res.Should().BeEmpty();
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

            var skip = new[] {"Vehicle", "Name", "Courses[1].Name"};
            var result = expected.GetDifferenceBetweenObjects(actual,
                str => str.Set(x => x.Courses[0].Duration, (act, exp) => act > TimeSpan.FromHours(3),
                    new Display {Expected = "Expected that Duration should be more that 3 hours"}), skip);
            var expectedDistinctionsCollection = new DistinctionsCollection()
                .Add(new Distinction("Courses[0].Duration", "Expected that Duration should be more that 3 hours",
                    "04:00:00"));

            CollectionAssert.AreEquivalent(result, expectedDistinctionsCollection);
        }

        [Test]
        public void CheckWithCustomRule()
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
                        Name = "Math",
                        Duration = TimeSpan.FromHours(5)
                    }
                }
            };

            var expected = new Student
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
                        Duration = TimeSpan.FromHours(3)
                    },
                    new Course
                    {
                        Name = "Fake",
                        Duration = TimeSpan.FromHours(4)
                    }
                }
            };

            var newRule = new Comparator.Implementations.Comparator();
            newRule.RuleForReferenceTypes.Add(new CourseRule());
            var result = ComparatorExtension.GetDifferenceBetweenObjects(expected, actual, newRule);
            var expectedDistinctionsCollection =
                new DistinctionsCollection().Add(new Distinction("Courses[1]", "Fake", "Math"));

            CollectionAssert.AreEquivalent(result, expectedDistinctionsCollection);

        }

        [Test]
        public void DictionaryVerifications()
        {
            var exp = new Library
            {
                Books = new Dictionary<string, Book>
                {
                    ["hobbit"] = new Book {Pages = 1000, Text = "hobbit Text"},
                    ["murder in orient express"] = new Book {Pages = 500, Text = "murder in orient express Text"},
                    ["Shantaram"] = new Book {Pages = 500, Text = "Shantaram Text"}
                }
            };

            var act = new Library
            {
                Books = new Dictionary<string, Book>
                {
                    ["hobbit"] = new Book {Pages = 1, Text = "hobbit Text"},
                    ["murder in orient express"] = new Book {Pages = 500, Text = "murder in orient express Text1"},
                    ["Shantaram"] = new Book {Pages = 500, Text = "Shantaram Text"}
                }
            };

            var result = exp.GetDifferenceBetweenObjects(act);
            var expectedDistinctionsCollection = new DistinctionsCollection()
                .Add(new Distinction("Books[hobbit].Pages", 1000, 1)).Add(new Distinction(
                    "Books[murder in orient express].Text", "murder in orient express Text",
                    "murder in orient express Text1"));
            CollectionAssert.AreEquivalent(result, expectedDistinctionsCollection);
        }
    }
}
