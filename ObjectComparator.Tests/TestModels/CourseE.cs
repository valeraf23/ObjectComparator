using System;

namespace ObjectsComparator.Tests.TestModels
{
    internal class CourseE
    {
        public string Name { get; set; }
        public TimeSpan? Duration { get; set; }

        public override bool Equals(object obj)
        {
            return Equals((CourseE) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Duration);
        }

        public bool Equals(CourseE obj)
        {
            return this.Name == obj.Name;
        }
    }
}