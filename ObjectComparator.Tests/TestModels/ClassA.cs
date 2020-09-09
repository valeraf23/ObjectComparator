using System.Collections.Generic;

namespace ObjectsComparator.Tests.TestModels
{
    internal class ClassA
    {
        public string One { get; set; }
        public int? Two { get; set; }
        public string[] ArrayThird { get; set; }
        public SomeClass[] InnerClass { get; set; }
    }
   
    internal class ClassB
    {
        public string One { get; set; }
        public SomeClass Two { get; set; }
        public SomeClass Third { get; set; }
    }
    internal class ClassC
    {
        public string One { get; set; }
        public int? Two { get; set; }
        public IEnumerable<string> ArrayThird { get; set; }
        public HashSet<string> InnerClass { get; set; }
    }
}