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

    internal class GroupPortals
    {
        public List<int> Portals { get; set; }
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