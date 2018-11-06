using System.Collections.Generic;

namespace ObjectsComparator.Tests.TestModels
{
    internal class Library
    {
        public Dictionary<string, Book> Books { get; set; } = new Dictionary<string, Book>();
    }
}