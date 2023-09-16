using System.Collections.Generic;

namespace ObjectsComparator.Tests.TestModels
{
    internal class BuildingEnumerable
    {
        public string Address { get; set; }

        public IEnumerable<int> ListOfAppNumbers { get; set; }
    }
}