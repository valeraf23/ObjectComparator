using FluentAssertions;
using NUnit.Framework;
using ObjectsComparator.Comparator.Helpers;
using ObjectsComparator.Tests.TestModels;
using System;

namespace ObjectsComparator.Tests;

[TestFixture]
public class UnifiedConfigurationTests
{
    [Test]
    public void DeeplyEquals_WithStringTypeStrategy_TreatsNullAndEmptyAsEqual()
    {
        var expected = new VehicleEntity
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
            config => config.WithTypeStrategies(s => s.Set(typeof(string), (e, a) =>
                (string.IsNullOrEmpty((string?)e) && string.IsNullOrEmpty((string?)a)) ||
                string.Equals((string?)e, (string?)a, StringComparison.OrdinalIgnoreCase))));

        result.Should().BeEmpty();
    }

    [Test]
    public void DeeplyEquals_UnifiedConfig_CombinesAllOptions()
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

        var result = expected.DeeplyEquals(actual, config => config
            .AllowDifferentTypes()
            .Ignore("InternalCode")
            .WithTypeStrategies(ts => ts.Set<string>((e, a) =>
                (string.IsNullOrEmpty(e) && string.IsNullOrEmpty(a)) || e == a)));

        result.Should().BeEmpty();
    }

    [Test]
    public void DeeplyEquals_UnifiedConfig_WithPropertyAndTypeStrategies()
    {
        var expected = new VehicleEntity
        {
            Id = 1,
            Model = "BMW",
            Description = "Same",
            InternalCode = "code"
        };

        var actual = new VehicleEntity
        {
            Id = 1,
            Model = "BMW",
            Description = "Same",
            InternalCode = "CODE"
        };

        var result = expected.DeeplyEquals(actual, config => config
            .WithTypeStrategies(ts => ts.Set<string>((e, a) =>
                string.Equals(e, a, StringComparison.OrdinalIgnoreCase))));

        result.Should().BeEmpty();
    }
}