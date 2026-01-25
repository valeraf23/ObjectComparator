namespace ObjectsComparator.Tests.TestModels;

internal class StudentIName : IName
{
    public int Age { get; set; }
    public Vehicle Vehicle { get; set; }
    public Course[] Courses { get; set; }
    public string Name { get; set; }
}