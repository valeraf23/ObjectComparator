namespace ObjectsComparator.Tests.TestModels
{
    internal class StudentDto
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public CourseDto[] Courses { get; set; }
    }

    internal class CourseDto
    {
        public string Name { get; set; }
        public int Credits { get; set; }
    }

    internal class StudentEntity
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public CourseEntity[] Courses { get; set; }
    }

    internal class CourseEntity
    {
        public string Name { get; set; }
        public int Credits { get; set; }
    }
}
