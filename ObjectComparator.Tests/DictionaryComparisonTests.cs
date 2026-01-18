using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Tests.TestModels;
using System.Collections.Generic;

namespace ObjectsComparator.Tests
{
    [TestFixture]
    public class DictionaryComparisonTests
    {
        [Test]
        public void DictionaryVerifications()
        {
            var exp = new Library
            {
                Books = new Dictionary<string, Book>
                {
                    ["hobbit"] = new() { Pages = 1000, Text = "hobbit Text" },
                    ["murder in orient express"] = new() { Pages = 500, Text = "murder in orient express Text" },
                    ["Shantaram"] = new() { Pages = 500, Text = "Shantaram Text" }
                }
            };

            var act = new Library
            {
                Books = new Dictionary<string, Book>
                {
                    ["hobbit"] = new() { Pages = 1, Text = "hobbit Text" },
                    ["murder in orient express"] = new() { Pages = 500, Text = "murder in orient express Text1" },
                    ["Shantaram"] = new() { Pages = 500, Text = "Shantaram Text" },
                    ["Shantaram1"] = new() { Pages = 500, Text = "Shantaram Text" }
                }
            };

            var result = exp.DeeplyEquals(act);
            var expectedDistinctionsCollection = DeepEqualityResult.Create(new[]
            {
                new Distinction("Library.Books", null, "Shantaram1", "Added"),
                new Distinction("Library.Books[hobbit].Pages", 1000, 1),
                new Distinction(
                    "Library.Books[murder in orient express].Text", "murder in orient express Text",
                    "murder in orient express Text1")
            });

            CollectionAssert.AreEquivalent(result, expectedDistinctionsCollection);
        }

        [Test]
        public void DictionaryVerifications_Different_length()
        {
            var exp = new Library3
            {
                Books = new Dictionary<SomeKey, Book>
                {
                    [new SomeKey("hobbit")] = new() { Pages = 1000, Text = "hobbit Text" },
                    [new SomeKey("murder in orient express")] =
                        new() { Pages = 500, Text = "murder in orient express Text" },
                    [new SomeKey("Shantaram")] = new() { Pages = 500, Text = "Shantaram Text" }
                }
            };

            var act = new Library3
            {
                Books = new Dictionary<SomeKey, Book>
                {
                    [new SomeKey("hobbit")] = new() { Pages = 1, Text = "hobbit Text" },
                }
            };

            var result = exp.DeeplyEquals(act);
            var expectedDistinctionsCollection = DeepEqualityResult.Create(new[]
            {
                new Distinction("Library3.Books",
                    $"{new SomeKey("murder in orient express")}, {new SomeKey("Shantaram")}", null, "Removed"),
                new Distinction("Library3.Books[SomeKey { Key = hobbit }].Pages", 1000, 1)
            });

            CollectionAssert.AreEquivalent(result, expectedDistinctionsCollection);
        }

        [Test]
        public void AreDeeplyEqualShouldReportCorrectlyWithDictionaries()
        {
            var firstDictionary = new Dictionary<string, string>
            {
                { "Key", "Value" },
                { "AnotherKey", "Value" },
            };

            var secondDictionary = new Dictionary<string, string>
            {
                { "Key", "Value" },
                { "AnotherKey", "AnotherValue" },
            };

            firstDictionary.DeeplyEquals(secondDictionary)[0].Should()
                .Be(new Distinction("Dictionary<String, String>[AnotherKey]", "Value", "AnotherValue"));
        }

        [Test]
        public void CompareIDictionaryProperty()
        {
            var exp = new Library2
            {
                Books = new Dictionary<string, Book>
                {
                    ["hobbit"] = new() { Pages = 1000, Text = "hobbit Text" },
                }
            };

            var act = new Library2
            {
                Books = new Dictionary<string, Book>
                {
                    ["hobbit"] = new() { Pages = 1, Text = "hobbit Text" },
                }
            };

            var result = exp.DeeplyEquals(act);
            var expectedDistinctionsCollection = DeepEqualityResult.Create(new[]
            {
                new Distinction("Library2.Books[hobbit].Pages", 1000, 1),
            });

            CollectionAssert.AreEquivalent(result, expectedDistinctionsCollection);
        }

        [Test]
        public void CompareIDictionaryImplementation()
        {
            var firstDictionary = new StringDictionary
            {
                { "Key", "Value" },
                { "AnotherKey", "Value" },
            };

            var secondDictionary = new StringDictionary
            {
                { "Key", "Value" },
                { "AnotherKey", "AnotherValue" },
            };

            firstDictionary.DeeplyEquals(secondDictionary)[0].Should()
                .Be(new Distinction("StringDictionary[AnotherKey]", "Value", "AnotherValue"));
        }

        [Test]
        public void CompareIDictionaryImplementationAsObject()
        {
            object firstDictionary = new StringDictionary
            {
                { "Key", "Value" },
                { "AnotherKey", "Value" },
            };

            object secondDictionary = new StringDictionary
            {
                { "Key", "Value" },
                { "AnotherKey", "AnotherValue" },
            };

            firstDictionary.DeeplyEquals(secondDictionary)[0].Should()
                .Be(new Distinction("StringDictionary[AnotherKey]", "Value", "AnotherValue"));
        }

        [Test]
        public void DictionaryVerifications_Complex_keys_produce_detailed_differences()
        {
            var sharedKey = new OpaqueKey { Id = 1, Name = "shared" };

            var expected = new LibraryWithOpaqueKeys
            {
                Books = new Dictionary<OpaqueKey, Book>
                {
                    [sharedKey] = new() { Pages = 100, Text = "same" },
                    [new OpaqueKey { Id = 2, Name = "missing" }] = new() { Pages = 200, Text = "expected only" }
                }
            };

            var act = new LibraryWithOpaqueKeys
            {
                Books = new Dictionary<OpaqueKey, Book>
                {
                    [sharedKey] = new() { Pages = 101, Text = "same" },
                    [new OpaqueKey { Id = 3, Name = "extra" }] = new() { Pages = 300, Text = "actual only" }
                }
            };

            var result = expected.DeeplyEquals(act);

            var sharedKeyPath = $"LibraryWithOpaqueKeys.Books[{SerializeForDiff(sharedKey)}]";
            var missingKeyPath =
                $"LibraryWithOpaqueKeys.Books[{SerializeForDiff(new OpaqueKey { Id = 2, Name = "missing" })}]";
            var extraKeyPath =
                $"LibraryWithOpaqueKeys.Books[{SerializeForDiff(new OpaqueKey { Id = 3, Name = "extra" })}]";

            var expectedDistinctionsCollection = DeepEqualityResult.Create(new[]
            {
                new Distinction(extraKeyPath, null,
                    SerializeForDiff(new Book { Pages = 300, Text = "actual only" }), "Added"),
                new Distinction(missingKeyPath,
                    SerializeForDiff(new Book { Pages = 200, Text = "expected only" }), null, "Removed"),
                new Distinction($"{sharedKeyPath}.Pages", 100, 101)
            });

            CollectionAssert.AreEquivalent(result, expectedDistinctionsCollection);
        }

        private static readonly JsonSerializerSettings CamelCaseIndentedSettings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented
        };

        private static string SerializeForDiff(object value) =>
            JsonConvert.SerializeObject(value, CamelCaseIndentedSettings);
    }
}
