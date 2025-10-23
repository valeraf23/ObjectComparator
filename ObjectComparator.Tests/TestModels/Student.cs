using System.Collections.Generic;

namespace ObjectsComparator.Tests.TestModels
{
    internal class Student
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Vehicle Vehicle { get; set; }
        public Course[] Courses { get; set; }
    }
    internal class LibraryWithOpaqueKeys
    {
        public Dictionary<OpaqueKey, Book> Books { get; set; } = new();
    }
    internal class OpaqueKey
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not OpaqueKey other) return false;
            return Id == other.Id && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return EqualityComparer<int>.Default.GetHashCode(Id) * 397 ^ (Name?.GetHashCode() ?? 0);
        }
    }
    internal class GroupPortals1
    {
        public List<Course> Courses { get; set; }
    }

    internal class GroupPortals
    {
        public List<int> Portals { get; set; }
        public List<GroupPortals1> Portals1 { get; set; }
    }

    internal class StudentNew
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Vehicle Vehicle { get; set; }
        public CourseNew[] Courses { get; set; }
    }

    internal record SomeKey(string Key);

    internal class StudentNew2
    {
        public CourseNew2 Name { get; set; }
    }
}