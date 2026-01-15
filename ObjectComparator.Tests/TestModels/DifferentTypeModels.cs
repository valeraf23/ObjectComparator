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

    // Models for testing combined strategies + options + ignore with different types
    internal class VehicleDto
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public string Description { get; set; }
        public string InternalCode { get; set; }
    }

    internal class VehicleEntity
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public string Description { get; set; }
        public string InternalCode { get; set; }
    }
}
