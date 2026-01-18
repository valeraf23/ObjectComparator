using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ObjectsComparator.Tests.TestModels;

internal class StringDictionary : IDictionary<string, string>
{
    private readonly Dictionary<string, string> _dictionary = new();

    public string this[string key]
    {
        get => _dictionary[key];
        set => _dictionary[key] = value;
    }

    public int Count => _dictionary.Count;

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }

    public void Add(string key, string value)
    {
        _dictionary.Add(key, value);
    }

    public ICollection<string> Keys => _dictionary.Keys;
    public ICollection<string> Values => _dictionary.Values;
    public bool IsReadOnly => throw new NotImplementedException();

    public void Add(KeyValuePair<string, string> item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(KeyValuePair<string, string> item)
    {
        throw new NotImplementedException();
    }

    public bool ContainsKey(string key)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(string key)
    {
        throw new NotImplementedException();
    }

    public bool Remove(KeyValuePair<string, string> item)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}