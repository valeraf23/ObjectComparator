using System.Collections;
using System.Collections.Generic;

namespace ObjectsComparator.Tests.TestModels
{
    public class StringList : IEnumerable<string>
    {
        private readonly List<string> _list = new();

        public void Add(string item) => _list.Add(item);

        public IEnumerator<string> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}