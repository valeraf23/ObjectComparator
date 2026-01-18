using System.Collections;
using System.Collections.Generic;

namespace ObjectsComparator.Tests.TestModels;

public class StringList : IEnumerable<string>
{
    private readonly List<string> _list = new();

    public IEnumerator<string> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(string item)
    {
        _list.Add(item);
    }
}