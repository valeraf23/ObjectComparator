using System;
using System.Collections.Generic;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;

namespace ObjectsComparator.Tests.TestModels
{
    public class CourseRule : ICompareObjectStrategy
    {
        public Distinctions Compare<T>(T expected, T actual, string propertyName)
        {
            var a = expected.ChangeType<Course>();
            var b = actual.ChangeType<Course>();
            return
                Distinctions
                    .CreateFor<string>(propertyName, a.Name, b.Name).WhenNot((x, y) => x == y);
        }


        public bool IsValid(Type member)
        {
            return member == typeof(Course);
        }

        public Func<string, bool> Ignore { get; set; }
        public IDictionary<string, ICompareValues> Strategies { get; set; }
    }
}