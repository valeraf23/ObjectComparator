using FluentAssertions;
using NUnit.Framework;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Tests.TestModels;
using System.Collections.Generic;
using System.Linq;

namespace ObjectsComparator.Tests
{
    [TestFixture]
    public class DifferentTypesComparisonTests
    {
        [Test]
        public void DeeplyEquals_ShouldCompareDifferentTypes_WhenAllowed()
        {
            var expected = new StudentDto
            {
                Name = "Alex",
                Age = 20,
                Courses = new[] { new CourseDto { Name = "Math", Credits = 3 } }
            };

            var actual = new StudentEntity
            {
                Name = "Alex",
                Age = 21,
                Courses = new[] { new CourseEntity { Name = "Math", Credits = 3 } }
            };

            var result = expected.DeeplyEquals(actual, config => config.AllowDifferentTypes());

            result.Should().BeEquivalentTo(new[]
            {
                new Distinction("StudentDto.Age", 20, 21)
            });
        }

        [Test]
        public void DeeplyEquals_ShouldCompareDifferentTypeCollections_WhenAllowed()
        {
            var expected = new[]
            {
                new StudentDto
                {
                    Name = "Alex",
                    Age = 20,
                    Courses = new[] { new CourseDto { Name = "Math", Credits = 3 } }
                }
            };

            var actual = new[]
            {
                new StudentEntity
                {
                    Name = "Alex",
                    Age = 20,
                    Courses = new[] { new CourseEntity { Name = "Math", Credits = 3 } }
                }
            };

            var result = expected.DeeplyEquals(actual, config => config.AllowDifferentTypes());

            result.Should().BeEmpty();
        }

        [Test]
        public void DeeplyEqualsIgnoreObjectTypes_ShouldCompareDifferentTypes()
        {
            var expected = new StudentDto
            {
                Name = "Alex",
                Age = 20,
                Courses = new[] { new CourseDto { Name = "Math", Credits = 3 } }
            };

            var actual = new StudentEntity
            {
                Name = "Alex",
                Age = 20,
                Courses = new[] { new CourseEntity { Name = "Math", Credits = 3 } }
            };

            var result = expected.DeeplyEquals(actual, config => config.AllowDifferentTypes());

            result.Should().BeEmpty();
        }

        [Test]
        public void DeeplyEqualsIgnoreObjectTypes_ShouldIgnoreCollectionMember_WhenIgnoreTokenOmitsIndex()
        {
            var expected = new List<ClassA>
            {
                new() { One = "1" },
                new() { Two = 2 }
            };

            var actual = new List<ClassB>
            {
                new() { One = "1" },
                new() { Two = new SomeClass() }
            };

            var result = expected.DeeplyEquals(actual, config => config.AllowDifferentTypes().Ignore("Two"));

            result.Should().BeEmpty();
        }

        [Test]
        public void DeeplyEquals_DifferentTypes_WithStrategiesOptionsAndIgnore_ShouldWork()
        {
            var expected = new VehicleDto
            {
                Id = 1,
                Model = "",
                Description = "Test",
                InternalCode = "ABC"
            };

            var actual = new VehicleEntity
            {
                Id = 1,
                Model = null,
                Description = "Test",
                InternalCode = "XYZ"
            };

            var result = expected.DeeplyEquals(actual,
                strategy => strategy
                    .Set(x => x.Model, (exp, act) =>
                        (string.IsNullOrEmpty(exp) && string.IsNullOrEmpty(act)) || exp == act),
                options => options.AllowDifferentTypes(),
                "InternalCode");

            result.Should().BeEmpty();
        }

        [Test]
        public void DeeplyEquals_DifferentTypes_WithStrategiesOptionsAndIgnore_ShouldReportDifferences()
        {
            var expected = new VehicleDto
            {
                Id = 1,
                Model = "",
                Description = "Expected Description",
                InternalCode = "ABC"
            };

            var actual = new VehicleEntity
            {
                Id = 1,
                Model = null,
                Description = "Actual Description",
                InternalCode = "XYZ"
            };

            var result = expected.DeeplyEquals(actual,
                strategy => strategy
                    .Set(x => x.Model, (exp, act) =>
                        (string.IsNullOrEmpty(exp) && string.IsNullOrEmpty(act)) || exp == act),
                options => options.AllowDifferentTypes(),
                "InternalCode");

            result.Should().HaveCount(1);
            result.First().Path.Should().EndWith("Description");
            result.First().ExpectedValue.Should().Be("Expected Description");
            result.First().ActualValue.Should().Be("Actual Description");
        }

        [Test]
        public void DeeplyEquals_DifferentTypes_WithMultipleStrategies_ShouldApplyAll()
        {
            var expected = new VehicleDto
            {
                Id = 1,
                Model = "",
                Description = null,
                InternalCode = "CODE"
            };

            var actual = new VehicleEntity
            {
                Id = 1,
                Model = null,
                Description = "",
                InternalCode = "CODE"
            };

            var result = expected.DeeplyEquals(actual,
                strategy => strategy
                    .Set(x => x.Model, (exp, act) =>
                        (string.IsNullOrEmpty(exp) && string.IsNullOrEmpty(act)) || exp == act)
                    .Set(x => x.Description, (exp, act) =>
                        (string.IsNullOrEmpty(exp) && string.IsNullOrEmpty(act)) || exp == act),
                options => options.AllowDifferentTypes());

            result.Should().BeEmpty();
        }
    }
}
