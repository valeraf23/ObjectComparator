namespace ObjectsComparator.Tests.TestModels
{
    internal class StudentIName: IName
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public Vehicle Vehicle { get; set; }
        public Course[] Courses { get; set; }
    }
}