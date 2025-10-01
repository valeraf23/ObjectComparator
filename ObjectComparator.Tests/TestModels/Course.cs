using System;

namespace ObjectsComparator.Tests.TestModels
{
    internal class Course
    {
        public string Name { get; set; }
        public TimeSpan? Duration { get; set; }
    }

    internal class CourseNew
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
    }

    internal class CourseNew2
    {
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }

        public static bool operator == (CourseNew2 a, CourseNew2 b)
        {
            return a?.Name == b?.Name && a?.Duration == b?.Duration;
        }

        public static bool operator !=(CourseNew2 a, CourseNew2 b)
        {
            return !(a == b);
        }
    }

    internal class CourseNew3
    {
        public string Name { get; set; }
        public int Duration { get; set; }

        public static bool operator ==(CourseNew3 a, CourseNew3 b) => a?.Name == b?.Name;
        public static bool operator !=(CourseNew3 a, CourseNew3 b) => !(a == b);
        public override bool Equals(object? obj) => obj is CourseNew3 other && this == other;
        public override int GetHashCode() => Name?.GetHashCode() ?? 0;
    }
}
