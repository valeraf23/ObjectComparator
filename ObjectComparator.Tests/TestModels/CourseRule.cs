using System;
using System.Collections.Generic;
using ObjectsComparator.Comparator;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;

namespace ObjectsComparator.Tests.TestModels
{
    public class CourseRule : ICompareObjectStrategy
    {
        public Distinctions Compare<T>(T valueA, T valueB, string propertyName)
        {
            var a = valueA.ChangeType<Course>();
            var b = valueB.ChangeType<Course>();
            return
                Distinctions
                    .CreateFor<string>(propertyName, a.Name, b.Name).WhenNot((x, y) => x == y);
        }


        public bool IsValid(Type member) => member == typeof(Course);
        public IList<string> Ignore { get; set; }
        public IDictionary<string, ICompareValues> Strategies { get; set; }
    }
}