using System;
using System.Collections.Generic;
using ObjectComparator.Comparator;
using ObjectComparator.Comparator.Interfaces;

namespace ObjectComparator.Tests.TestModels
{
    public class CourseRule : ICompareObjectStrategy
    {
        public DistinctionsCollection Compare<T>(T valueA, T valueB, string propertyName)
        {
            var a = (Course) (object) valueA;
            var b = (Course) (object) valueB;
            return a.Name == b.Name
                ? new DistinctionsCollection()
                : new DistinctionsCollection(new[] {new Distinction(propertyName, a.Name, b.Name)});
        }

        public bool IsValid(Type member) => member == typeof(Course);

        public IList<string> Ignore { get; set; }
        public IDictionary<string, ICompareValues> Strategies { get; set; }
    }
}