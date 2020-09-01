using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using FluentAssertions;
using ObjectsComparator.Comparator;
using ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties;
using ObjectsComparator.Tests.TestModels;

namespace PerformanceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<PerformanceTests>();
        }
    }


    [MemoryDiagnoser]
    // [DisassemblyDiagnoser(maxDepth:3,exportCombinedDisassemblyReport:true)]
    //  [SimpleJob(RuntimeMoniker.NetCoreApp31,launchCount: 10, warmupCount: 0, targetCount: 100)]
    [SimpleJob(RunStrategy.ColdStart, launchCount: 100)]
    public class PerformanceTests
    {
        private readonly Time _time = new Time("2016", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34);

        private readonly DigitalClock _dDigitalClock = new DigitalClock(true, new[] {1, 2},
            new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34)), "2015", 1.2F, 11, 1.12,
            new List<string> {"df", "asd"}, 1, 9);

        [Benchmark]
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

            var newRule = new Comparator();
            newRule.RuleForReferenceTypes.Add(new CourseRule());
            var result = ComparatorExtension.GetDistinctions(expected, actual, newRule);
        }

        [Benchmark]
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

            var result = exp.GetDistinctions(act);
        }

        [Benchmark]
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
            var result = expected.GetDistinctions(actual,
                str => str.Set(x => x.Courses[0].Duration, (act, exp) => act > TimeSpan.FromHours(3),
                    new Display {Expected = "Expected that Duration should be more that 3 hours"}), skip);
        }

        [Benchmark]
        public void DistinctionExist()
        {
            var time1 = new Time("wrong", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34);
            var resultNoDiffTime1 = _time.GetDistinctions(time1);
        }

        [Benchmark]
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

            var res = act.GetDistinctions(exp);
        }

        [Benchmark]
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

            var res = act.GetDistinctions(exp);
        }

        [Benchmark]
        public void IsObjectsEqualTest()
        {
            var time1 = new Time("2016", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34);
            var resultNoDiffTime1 = _time.GetDistinctions(time1);
        }

        [Benchmark]
        public void IsObjectsEqualTest_AllDigitalClock()
        {
            var d1DigitalClock = new DigitalClock(true, new[] {1, 2},
                new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34)), "2015", 1.2F, 11,
                1.12,
                new List<string> {"df", "asd"}, 1, 9);
            var resultNoDiffClock = _dDigitalClock.GetDistinctions(d1DigitalClock);
            resultNoDiffClock.Should().BeEmpty();
        }

        [Benchmark]
        public void IsObjectsEqualTestWhenIgnore()
        {
            var time1 = new Time("wrong", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34);
            var resultNoDiffTime1 = _time.GetDistinctions(time1, "PropYear");
        }

        [Benchmark]
        public void IsObjectsEqualTestWhenIgnoreInnerObject()
        {
            var d2DigitalClock = new DigitalClock(true, new[] {1, 2},
                new Calendar(3, new Time("wrong", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34)), "2015", 1.2F, 11,
                1.12,
                new List<string> {"df", "asd"}, 1, 9);
            var resultNoDiffTime1 =
                _dDigitalClock.GetDistinctions(d2DigitalClock, "PropCalendar.PropTimePanel.PropYear");
        }

        [Benchmark]
        public void IsObjectsEqualTestWhenSetStrategyInnerObject()
        {
            const int page = 4;
            var d2DigitalClock = new DigitalClock(true, new[] {1, 2},
                new Calendar(4, new Time("2016", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34)), "2015", 1.2F, 11,
                1.12,
                new List<string> {"df", "asd"}, 1, 9);
            var str = new Strategies<DigitalClock>().Set(x => x.PropCalendar.Page,
                (s, s1) => s1 == page);
            var resultNoDiffTime1 = _dDigitalClock.GetDistinctions(d2DigitalClock, str);
        }

        [Benchmark]
        public void IsObjectsEqualTestWhenSeveralIgnore()
        {
            var time1 = new Time("wrong", 1.5F, 77, 1.2, new List<string> {"", ""}, 4, 34);
            var resultNoDiffTime1 = _time.GetDistinctions(time1, "PropYear", "Day");
        }

        [Benchmark]
        public void IsObjectsFallTest_DigitalClock_ArrayDiff()
        {
            const int act = 11;
            var d3DigitalClock = new DigitalClock(true, new[] {act, 2},
                new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34)), "2015", 1.2F, 11,
                1.12,
                new List<string> {"df", "asd"}, 1, 9);
            var resultArrayDiffClock = _dDigitalClock.GetDistinctions(d3DigitalClock);
        }

        [Benchmark]
        public void IsObjectsFallTest_DigitalClock_BoolDiff()
        {
            const bool act = false;
            var d2DigitalClock = new DigitalClock(act, new[] {1, 2},
                new Calendar(3, new Time("2016", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34)), "2015", 1.2F, 11,
                1.12,
                new List<string> {"df", "asd"}, 1, 9);
            var resultBoolDiffClock = _dDigitalClock.GetDistinctions(d2DigitalClock);
        }

        [Benchmark]
        public void IsObjectsFallTest_Time_FloatDiff()
        {
            const float actual = 2.5F;
            var time3 = new Time("2016", actual, 3, 1.2, new List<string> {"", ""}, 4, 34);
            var resultFloatDiffTime1 = _time.GetDistinctions(time3);
        }

        [Benchmark]
        public void IsObjectsFallTest_Time_ListDiffTime()
        {
            const string actual = "ddd";
            var time6 = new Time("2016", 1.5F, 3, 1.2, new List<string> {"ddd", ""}, 4, 34);
            var resultCollectionDiffTime = _time.GetDistinctions(time6);
        }

        [Benchmark]
        public void IsObjectsFallTest_Time_ShortDiffTime()
        {
            const short actual = 1;
            var time4 = new Time("2016", 1.5F, actual, 1.2, new List<string> {"", ""}, 4, 34);
            var resultShortDiffTime = _time.GetDistinctions(time4);
        }

        [Benchmark]
        public void IsObjectsFallTest_Time_StringDiff()
        {
            const string actual = "2015";
            var time2 = new Time(actual, 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34);
            var resultStringDiffTime1 = _time.GetDistinctions(time2);
        }

        [Benchmark]
        public void ProperlyNamePropertiesForInnerObjects()
        {
            var d2DigitalClock = new DigitalClock(true, new[] {1, 2},
                new Calendar(3, new Time("wrong", 1.5F, 3, 1.2, new List<string> {"", ""}, 4, 34)), "2015", 1.2F, 11,
                1.12,
                new List<string> {"df", "asd"}, 1, 9);
            var resultNoDiffTime1 = _dDigitalClock.GetDistinctions(d2DigitalClock);

        }

        [Benchmark]
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

            var res = act.GetDistinctions(exp, str => str.Set(x => x.InnerClass[0].Foo,
                (s, s1) => s == data));
        }

        [Benchmark]
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

            var res = act.GetDistinctions(exp, str => str.Set(x => x.InnerClass[0].Foo,
                (s, s1) => s == data));
        }

        [Benchmark]
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

            var res = act.GetDistinctions(exp, st => st.Set(x => x.Two,
                (s, s1) => s == 5));
        }

        [Benchmark]
        public void Set_Strategy_For_Member_Array()
        {
            var act = new Building
            {
                Address = "NY, First Street",
                ListOfAppNumbers = new[] {32, 25, 14, 89}
            };

            var exp = new Building
            {
                Address = "NY, First Street",
                ListOfAppNumbers = new[] {555, 25, 14, 89}
            };

            const int expNumber = 32;

            var str = new Strategies<Building>().Set(x => x.ListOfAppNumbers[0],
                (ex, ac) => ex == expNumber);

            var res = act.GetDistinctions(exp, str);
        }

        [Benchmark]
        public void Set_Strategy_For_Member_List()
        {
            var act = new BuildingList
            {
                Address = "NY, First Street",
                ListOfAppNumbers = new[] {32, 25, 14, 89}
            };

            var exp = new BuildingList
            {
                Address = "NY, First Street",
                ListOfAppNumbers = new[] {555, 25, 14, 89}
            };

            const int expNumber = 32;

            var str = new Strategies<BuildingList>().Set(x => x.ListOfAppNumbers[0],
                (ex, ac) => ex == expNumber);

            var res = act.GetDistinctions(exp, str);
        }

        [Benchmark]
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

            var res = act.GetDistinctions(exp, str => str.Set(x => x.Third,
                (s, s1) => s.Foo == "yes"));
        }

        [Benchmark]
        public void Set_Strategy_For_Null_Member()
        {
            var act = new ClassB
            {
                One = "f",
                Two = new SomeClass {Foo = "no"},
                Third = null
            };

            var exp = new ClassB
            {
                One = "f",
                Two = new SomeClass {Foo = "no"},
                Third = new SomeClass {Foo = "yes"}
            };

            var res = act.GetDistinctions(exp, str => str.Set(x => x.Third,
                (actual, expected) => expected.Foo == "yes"));

        }

        [Benchmark]
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

            var res = act.GetDistinctions(exp, propName => propName.EndsWith("Name"));
        }

        [Benchmark]
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

            var actual = act.GetDistinctions(exp, propName => propName == "Name");

        }
    }
}
