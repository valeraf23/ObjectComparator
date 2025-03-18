using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ObjectsComparator.Comparator.RepresentationDistinction;
using System;
using System.Collections.Generic;

namespace ObjectsComparator.Comparator.Helpers;

internal static class DeepComparisonJsonConverter
{
    public static string ToJson(this DeepEqualityResult distinctions)
    {
        var result = new Dictionary<string, object>();

        foreach (var distinction in distinctions)
        {
            var segments = ParsePath(distinction.Path);
            AddToResult(result, segments, distinction);
        }

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented
        };

        return JsonConvert.SerializeObject(result, settings);
    }

    private static void AddToResult(IDictionary<string, object> rootNode, List<PathSegment> segments,
        Distinction distinction)
    {
        var parentNode = rootNode;

        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            var isFinalSegment = i == segments.Count - 1;
            var key = GetSegmentKey(segment);

            if (isFinalSegment)
            {
                HandleFinalSegment(parentNode, segment, key, distinction);
                return;
            }

            parentNode = GetOrCreateChildDictionary(parentNode, segment, key);
        }
    }

    private static string GetSegmentKey(PathSegment segment)
    {
        return segment.IsDictionary ? segment.Key! :
            segment.IsArray ? segment.Index!.ToString() :
            segment.Name;
    }

    private static void HandleFinalSegment(IDictionary<string, object> parentNode, PathSegment segment, string key,
        Distinction distinction)
    {
        if (segment.IsDictionary || segment.IsArray)
        {
            var containerNode = EnsureDictionaryNode(parentNode, segment.Name);
            AddDistinctionToSelf(containerNode, key, distinction);
        }
        else if (distinction.Details is "Added" or "Removed")
        {
            var container = EnsureDictionaryNode(parentNode, key);
            AddDistinctionToSelf(container, distinction.Details, distinction);
        }
        else
        {
            parentNode[key] = new Distinctions
            {
                Before = distinction.ExpectedValue,
                After = distinction.ActualValue,
                Details = distinction.Details
            };
        }
    }

    private static Dictionary<string, object> GetOrCreateChildDictionary(IDictionary<string, object> parentNode,
        PathSegment segment, string key)
    {
        var childNodeDictionary = EnsureDictionaryNode(parentNode, segment.Name);

        if (!childNodeDictionary.TryGetValue(key, out var nextNode) ||
            nextNode is not Dictionary<string, object> dictionaryNode)
        {
            dictionaryNode = new Dictionary<string, object>();
            childNodeDictionary[key] = dictionaryNode;
        }

        return dictionaryNode;
    }


    private static void AddDistinctionToSelf(IDictionary<string, object> node, string key, Distinction distinction)
    {
        if (!node.TryGetValue(key, out var existing) || existing is not List<Distinctions>)
        {
            var list = new List<Distinctions>();
            node[key] = list;
        }

        ((List<Distinctions>)node[key]).Add(new Distinctions
        {
            Before = distinction.ExpectedValue,
            After = distinction.ActualValue,
            Details = distinction.Details
        });
    }

    private static Dictionary<string, object> EnsureDictionaryNode(IDictionary<string, object> parentNode, string key)
    {
        if (!parentNode.TryGetValue(key, out var existing) || existing is not Dictionary<string, object> dictionaryNode)
        {
            dictionaryNode = new Dictionary<string, object>();
            parentNode[key] = dictionaryNode;
        }

        return dictionaryNode;
    }

    private static List<PathSegment> ParsePath(string path)
    {
        var segments = new List<PathSegment>();
        var pathSpan = path.AsSpan();
        var dotIndex = pathSpan.IndexOf('.');

        if (dotIndex >= 0)
            pathSpan = pathSpan[(dotIndex + 1)..];
        else
            return segments;
        while ((dotIndex = pathSpan.IndexOf('.')) >= 0)
        {
            var partSpan = pathSpan[..dotIndex];
            pathSpan = pathSpan[(dotIndex + 1)..];
            AddPathSegment(partSpan, segments);
        }

        if (!pathSpan.IsEmpty) AddPathSegment(pathSpan, segments);

        return segments;
    }

    private static void AddPathSegment(ReadOnlySpan<char> partSpan, List<PathSegment> segments)
    {
        var bracketIndex = partSpan.IndexOf('[');
        if (bracketIndex >= 0)
        {
            var bracketEnd = partSpan.IndexOf(']');
            var nameSpan = partSpan[..bracketIndex];
            var bracketValueSpan = partSpan.Slice(bracketIndex + 1, bracketEnd - bracketIndex - 1);

            if (int.TryParse(bracketValueSpan, out var index))
                segments.Add(new PathSegment(nameSpan.ToString(), true, index));
            else
                segments.Add(new PathSegment(nameSpan.ToString(), isDictionary: true,
                    dictKey: bracketValueSpan.ToString()));
        }
        else
        {
            segments.Add(new PathSegment(partSpan.ToString()));
        }
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