using Newtonsoft.Json;
using ObjectsComparator.Comparator.RepresentationDistinction;
using System;
using System.Collections.Generic;

namespace ObjectsComparator.Comparator.Helpers;

internal static class DeepComparisonJsonConverter
{
    public static string ToJson(
        this DeepEqualityResult distinctions)
    {
        var result = new Dictionary<string, object>();

        foreach (var distinction in distinctions)
        {
            var segments = ParsePath(distinction.Path);
            AddToResult(result, segments, 0, distinction);
        }

        return JsonConvert.SerializeObject(result, Formatting.Indented);
    }

    private static void AddToResult(IDictionary<string, object> current, List<PathSegment> segments, int index, Distinction distinction)
    {
        while (true)
        {
            var segment = segments[index];
            var isFinalSegment = index == segments.Count - 1;

            if (isFinalSegment)
            {
                switch (segment.IsArray)
                {
                    case false when !segment.IsDictionary:
                    {
                        var newValue = new Distinctions { Before = distinction.ExpectedValue, After = distinction.ActualValue, Details = distinction.Details };

                        if (!current.TryAdd(segment.Name, newValue))
                        {
                            var value = current[segment.Name];
                            if (value is List<Distinctions> list)
                                list.Add(newValue);
                            else
                                current[segment.Name] = new List<Distinctions> { (Distinctions)value, newValue };
                        }

                        break;
                    }
                    case true:
                    {
                        var list = EnsureListNode(current, segment.Name);
                        EnsureListSize(list, segment.Index.Value);

                        if (list[segment.Index.Value] is Dictionary<string, object> dict)
                        {
                            AddDistinctionToSelf(dict, distinction);
                        }
                        else
                        {
                            var newDict = new Dictionary<string, object> { ["_self"] = new List<Distinctions> { new() { Before = distinction.ExpectedValue, After = distinction.ActualValue, Details = distinction.Details } } };
                            list[segment.Index.Value] = newDict;
                        }

                        break;
                    }
                    default:
                    {
                        if (segment.IsDictionary)
                        {
                            var dict = EnsureDictionaryNode(current, segment.Name);
                            if (dict.TryGetValue(segment.Key, out var existing) && existing is Dictionary<string, object> childDict)
                            {
                                AddDistinctionToSelf(childDict, distinction);
                            }
                            else
                            {
                                var newDict = new Dictionary<string, object> { ["_self"] = new List<Distinctions> { new() { Before = distinction.ExpectedValue, After = distinction.ActualValue, Details = distinction.Details } } };
                                dict[segment.Key] = newDict;
                            }
                        }

                        break;
                    }
                }
            }
            else
            {
                IDictionary<string, object>? nextDict = null;

                if (segment.IsArray)
                {
                    var list = EnsureListNode(current, segment.Name);
                    EnsureListSize(list, segment.Index.Value);
                    list[segment.Index.Value] ??= new Dictionary<string, object>();
                    nextDict = list[segment.Index.Value] as Dictionary<string, object>;
                }
                else if (segment.IsDictionary)
                {
                    var dict = EnsureDictionaryNode(current, segment.Name);
                    if (!dict.TryGetValue(segment.Key, out var child) || child == null)
                    {
                        child = new Dictionary<string, object>();
                        dict[segment.Key] = child;
                    }

                    nextDict = child as Dictionary<string, object>;
                }
                else
                {
                    if (!current.ContainsKey(segment.Name) || current[segment.Name] is Distinctions) current[segment.Name] = new Dictionary<string, object>();
                    nextDict = current[segment.Name] as Dictionary<string, object>;
                }

                current = nextDict;
                index = index + 1;
                continue;
            }

            break;
        }
    }

    private static void AddDistinctionToSelf(Dictionary<string, object> node, Distinction distinction)
    {
        if (!node.TryGetValue("_self", out var existing) || existing == null)
        {
            node["_self"] = new List<Distinctions>
            {
                new()
                {
                    Before = distinction.ExpectedValue,
                    After = distinction.ActualValue,
                    Details = distinction.Details
                }
            };
            return;
        }

        if (existing is Distinctions single)
        {
            node["_self"] = new List<Distinctions>
            {
                single,
                new()
                {
                    Before = distinction.ExpectedValue,
                    After = distinction.ActualValue,
                    Details = distinction.Details
                }
            };
            return;
        }

        if (existing is List<Distinctions> list)
        {
            list.Add(new Distinctions
            {
                Before = distinction.ExpectedValue,
                After = distinction.ActualValue,
                Details = distinction.Details
            });
            return;
        }

        throw new InvalidOperationException($"'_self' is of unexpected type: {existing.GetType().Name}");
    }

    private static Dictionary<string, object> EnsureDictionaryNode(IDictionary<string, object> current, string name)
    {
        if (!current.TryGetValue(name, out var existing) || existing == null)
        {
            var newDict = new Dictionary<string, object>();
            current[name] = newDict;
            return newDict;
        }

        switch (existing)
        {
            case Dictionary<string, object> dict:
                return dict;
            case Distinctions dist:
            {
                var newDict = new Dictionary<string, object>
                {
                    ["_self"] = new List<Distinctions> { dist }
                };
                current[name] = newDict;
                return newDict;
            }
            case List<Distinctions> listOfDistinctions:
            {
                var newDict = new Dictionary<string, object>
                {
                    ["_self"] = listOfDistinctions
                };
                current[name] = newDict;
                return newDict;
            }
            default:
                switch (existing)
                {
                    case List<object> list:
                    {
                        var newDict = new Dictionary<string, object>
                        {
                            ["_children"] = list
                        };
                        current[name] = newDict;
                        return newDict;
                    }
                    default:
                        throw new InvalidOperationException(
                            $"Cannot convert {existing.GetType().Name} into a Dictionary<string, object> at '{name}'.");
                }
        }
    }

    private static List<object> EnsureListNode(IDictionary<string, object> current, string name)
    {
        if (!current.TryGetValue(name, out var existing) || existing == null)
        {
            var newList = new List<object>();
            current[name] = newList;
            return newList;
        }

        switch (existing)
        {
            case List<object> list:
                return list;
            case Distinctions dist:
            {
                var dict = new Dictionary<string, object>
                {
                    ["_self"] = new List<Distinctions> { dist }
                };
                var newList = new List<object> { dict };
                current[name] = newList;
                return newList;
            }
            case Dictionary<string, object> existingDict:
            {
                var newList = new List<object> { existingDict };
                current[name] = newList;
                return newList;
            }
            default:
                throw new InvalidOperationException(
                    $"Cannot convert {existing.GetType().Name} into a List<object> at '{name}'.");
        }
    }

    private static void EnsureListSize(List<object?> list, int index)
    {
        while (list.Count <= index)
            list.Add(null);
    }

    private static List<PathSegment> ParsePath(string path)
    {
        var segments = new List<PathSegment>();
        var parts = path.Split('.');

        foreach (var part in parts)
            if (part.Contains("[") && part.Contains("]"))
            {
                var name = part[..part.IndexOf('[')];
                var bracketValue = part.Substring(
                    part.IndexOf('[') + 1,
                    part.IndexOf(']') - part.IndexOf('[') - 1);

                segments.Add(int.TryParse(bracketValue, out var idx)
                    ? new PathSegment(name, true, idx)
                    : new PathSegment(name, isDictionary: true, dictKey: bracketValue));
            }
            else
            {
                segments.Add(new PathSegment(part));
            }

        return segments;
    }

    private record Distinctions
    {
        public object? Before { get; set; }
        public object? After { get; set; }
        public string? Details { get; set; }
    }

    private record PathSegment
    {
        public PathSegment(string name, bool isArray = false, int? index = null, bool isDictionary = false,
            string? dictKey = null)
        {
            Name = name;
            IsArray = isArray;
            Index = index;
            IsDictionary = isDictionary;
            Key = dictKey;
        }

        public string Name { get; }
        public bool IsArray { get; }
        public int? Index { get; }
        public bool IsDictionary { get; }
        public string? Key { get; }
    }
}