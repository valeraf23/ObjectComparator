using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using System.Collections.Generic;

namespace ObjectsComparator.Tests
{
    [TestFixture]
    public class JsonSerializationTests
    {
        [Test]
        public void ToJson_ShouldSerializeDistinctionsCollectionCorrectly()
        {
            var distinctions = DeepEqualityResult.Create(new[]
            {
                new Distinction("Snapshot.Status", "Active", "Deprecated", "Different state"),
                new Distinction("Snapshot.Rules[2].Expression", "Amount > 100", "Amount > 200"),
                new Distinction("Snapshot.Rules[6].Name", "OldName", "NewName"),
                new Distinction("Snapshot.Rules[3]", "Rule-3", "Rule-3 v2"),
                new Distinction("Snapshot.Metadata[isEnabled]", true, false),
                new Distinction("Snapshot.Metadata[range of values].Min", 10, 20),
                new Distinction(
                    "Snapshot.Metadata[range of values].Bounds[1].Label", "Old bound", "New bound"),
                new Distinction("Snapshot.Portals[2]", null, 91, "Added"),
                new Distinction("Snapshot.Portals[3]", null, 101, "Added"),
                new Distinction("Snapshot.Portals[4]", 1000, null, "Removed"),
                new Distinction("Snapshot.Portals[0].Title", "Main Portal", "Main Portal v2"),
            });

            var actualJson = DeepEqualsExtension.ToJson(distinctions);

            Dictionary<string, object?> Expect(object? before, object? after, string details = "") =>
                new()
                {
                    ["before"] = before,
                    ["after"] = after,
                    ["details"] = details,
                };

            var expectedStructure = new Dictionary<string, object?>
            {
                ["Status"] = Expect("Active", "Deprecated", "Different state"),
                ["Rules"] = new Dictionary<string, object?>
                {
                    ["2"] = new Dictionary<string, object?>
                    {
                        ["Expression"] = Expect("Amount > 100", "Amount > 200"),
                    },
                    ["6"] = new Dictionary<string, object?>
                    {
                        ["Name"] = Expect("OldName", "NewName"),
                    },
                    ["3"] = Expect("Rule-3", "Rule-3 v2"),
                },
                ["Metadata"] = new Dictionary<string, object?>
                {
                    ["isEnabled"] = Expect(true, false),
                    ["range of values"] = new Dictionary<string, object?>
                    {
                        ["Min"] = Expect(10, 20),
                        ["Bounds"] = new Dictionary<string, object?>
                        {
                            ["1"] = new Dictionary<string, object?>
                            {
                                ["Label"] = Expect("Old bound", "New bound"),
                            },
                        },
                    },
                },
                ["Portals"] = new Dictionary<string, object?>
                {
                    ["2"] = Expect(null, 91, "Added"),
                    ["3"] = Expect(null, 101, "Added"),
                    ["4"] = Expect(1000, null, "Removed"),
                    ["0"] = new Dictionary<string, object?>
                    {
                        ["Title"] = Expect("Main Portal", "Main Portal v2"),
                    },
                },
            };

            var serializer = JsonSerializer.Create(SerializerSettings.Settings);
            var expectedJson = JToken.FromObject(expectedStructure, serializer).ToString();
            var actualJsonNormalized = JToken.Parse(actualJson).ToString();

            Assert.AreEqual(expectedJson, actualJsonNormalized);
        }
    }
}
