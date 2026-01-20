using FluentAssertions;
using NUnit.Framework;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Rules;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using System;
using System.Collections.Generic;

namespace ObjectsComparator.Tests;

[TestFixture]
public class CompareValuesPriorityTests
{
    private const string TestPropertyPath = "TestObject.Property";

    [Test]
    public void Compare_WhenPropertyIsIgnored_ReturnsNone()
    {
        // Arrange
        var defaultComparer = new TrackingComparer("default");
        var propertyStrategy = new TrackingComparer("property");
        var typeStrategy = new TrackingComparer("type");

        var strategies = new Dictionary<string, ICustomCompareValues>
        {
            { TestPropertyPath, propertyStrategy }
        };
        var typeStrategies = new Dictionary<Type, ICustomCompareValues>
        {
            { typeof(string), typeStrategy }
        };

        var compareValues = new CompareValues(
            defaultComparer,
            strategies,
            path => path == TestPropertyPath, // Ignore this property
            typeStrategies);

        // Act
        var result = compareValues.Compare("expected", "actual", TestPropertyPath);

        // Assert
        result.IsEmpty().Should().BeTrue("ignored properties should return None");
        defaultComparer.WasCalled.Should().BeFalse("default comparer should not be called when property is ignored");
        propertyStrategy.WasCalled.Should().BeFalse("property strategy should not be called when property is ignored");
        typeStrategy.WasCalled.Should().BeFalse("type strategy should not be called when property is ignored");
    }

    [Test]
    public void Compare_WhenPropertyStrategyExists_UsesPropertyStrategyOverTypeStrategy()
    {
        // Arrange
        var defaultComparer = new TrackingComparer("default");
        var propertyStrategy = new TrackingComparer("property");
        var typeStrategy = new TrackingComparer("type");

        var strategies = new Dictionary<string, ICustomCompareValues>
        {
            { TestPropertyPath, propertyStrategy }
        };
        var typeStrategies = new Dictionary<Type, ICustomCompareValues>
        {
            { typeof(string), typeStrategy }
        };

        var compareValues = new CompareValues(
            defaultComparer,
            strategies,
            _ => false, // Don't ignore anything
            typeStrategies);

        // Act
        var result = compareValues.Compare("expected", "actual", TestPropertyPath);

        // Assert
        propertyStrategy.WasCalled.Should().BeTrue("property strategy should be used first");
        typeStrategy.WasCalled.Should().BeFalse("type strategy should not be called when property strategy exists");
        defaultComparer.WasCalled.Should().BeFalse("default comparer should not be called when property strategy exists");
    }

    [Test]
    public void Compare_WhenNoPropertyStrategyButTypeStrategyExists_UsesTypeStrategy()
    {
        // Arrange
        var defaultComparer = new TrackingComparer("default");
        var typeStrategy = new TrackingComparer("type");

        var strategies = new Dictionary<string, ICustomCompareValues>(); // No property strategies
        var typeStrategies = new Dictionary<Type, ICustomCompareValues>
        {
            { typeof(string), typeStrategy }
        };

        var compareValues = new CompareValues(
            defaultComparer,
            strategies,
            _ => false, // Don't ignore anything
            typeStrategies);

        // Act
        var result = compareValues.Compare("expected", "actual", TestPropertyPath);

        // Assert
        typeStrategy.WasCalled.Should().BeTrue("type strategy should be used when no property strategy exists");
        defaultComparer.WasCalled.Should().BeFalse("default comparer should not be called when type strategy exists");
    }

    [Test]
    public void Compare_WhenNoStrategiesExist_UsesDefaultComparer()
    {
        // Arrange
        var defaultComparer = new TrackingComparer("default");

        var strategies = new Dictionary<string, ICustomCompareValues>(); // No property strategies
        var typeStrategies = new Dictionary<Type, ICustomCompareValues>(); // No type strategies

        var compareValues = new CompareValues(
            defaultComparer,
            strategies,
            _ => false, // Don't ignore anything
            typeStrategies);

        // Act
        var result = compareValues.Compare("expected", "actual", TestPropertyPath);

        // Assert
        defaultComparer.WasCalled.Should().BeTrue("default comparer should be used when no strategies exist");
    }

    [Test]
    public void Compare_WhenPropertyStrategyExistsForDifferentPath_UsesTypeStrategy()
    {
        // Arrange
        var defaultComparer = new TrackingComparer("default");
        var propertyStrategy = new TrackingComparer("property");
        var typeStrategy = new TrackingComparer("type");

        var strategies = new Dictionary<string, ICustomCompareValues>
        {
            { "DifferentPath", propertyStrategy }
        };
        var typeStrategies = new Dictionary<Type, ICustomCompareValues>
        {
            { typeof(string), typeStrategy }
        };

        var compareValues = new CompareValues(
            defaultComparer,
            strategies,
            _ => false,
            typeStrategies);

        // Act
        var result = compareValues.Compare("expected", "actual", TestPropertyPath);

        // Assert
        propertyStrategy.WasCalled.Should().BeFalse("property strategy for different path should not be called");
        typeStrategy.WasCalled.Should().BeTrue("type strategy should be used when property strategy doesn't match");
        defaultComparer.WasCalled.Should().BeFalse("default comparer should not be called when type strategy exists");
    }

    [Test]
    public void Compare_WhenTypeStrategyExistsForDifferentType_UsesDefaultComparer()
    {
        // Arrange
        var defaultComparer = new TrackingComparer("default");
        var typeStrategy = new TrackingComparer("type");

        var strategies = new Dictionary<string, ICustomCompareValues>();
        var typeStrategies = new Dictionary<Type, ICustomCompareValues>
        {
            { typeof(int), typeStrategy }
        };

        var compareValues = new CompareValues(
            defaultComparer,
            strategies,
            _ => false,
            typeStrategies);

        // Act
        var result = compareValues.Compare("expected", "actual", TestPropertyPath); // Comparing strings

        // Assert
        typeStrategy.WasCalled.Should().BeFalse("type strategy for different type should not be called");
        defaultComparer.WasCalled.Should().BeTrue("default comparer should be used when type strategy doesn't match");
    }

    [Test]
    public void Compare_WhenExpectedIsNullAndActualIsNotNull_ReturnsDistinctionWithNull()
    {
        // Arrange
        var defaultComparer = new TrackingComparer("default");

        var compareValues = new CompareValues(
            defaultComparer,
            new Dictionary<string, ICustomCompareValues>(),
            _ => false,
            new Dictionary<Type, ICustomCompareValues>());

        // Act
        var result = compareValues.Compare<string?>(null, "actual", TestPropertyPath);

        // Assert
        result.IsEmpty().Should().BeFalse();
        result.Should().ContainSingle()
            .Which.ExpectedValue.Should().Be("null");
        defaultComparer.WasCalled.Should().BeFalse("default comparer should not be called for null comparison");
    }

    [Test]
    public void Compare_WhenExpectedIsNotNullAndActualIsNull_ReturnsDistinctionWithNull()
    {
        // Arrange
        var defaultComparer = new TrackingComparer("default");

        var compareValues = new CompareValues(
            defaultComparer,
            new Dictionary<string, ICustomCompareValues>(),
            _ => false,
            new Dictionary<Type, ICustomCompareValues>());

        // Act
        var result = compareValues.Compare<string?>("expected", null, TestPropertyPath);

        // Assert
        result.IsEmpty().Should().BeFalse();
        result.Should().ContainSingle()
            .Which.ActualValue.Should().Be("null");
        defaultComparer.WasCalled.Should().BeFalse("default comparer should not be called for null comparison");
    }

    [Test]
    public void Compare_WhenBothValuesAreNull_ReturnsNone()
    {
        // Arrange
        var defaultComparer = new TrackingComparer("default");

        var compareValues = new CompareValues(
            defaultComparer,
            new Dictionary<string, ICustomCompareValues>(),
            _ => false,
            new Dictionary<Type, ICustomCompareValues>());

        // Act
        var result = compareValues.Compare<string?>(null, null, TestPropertyPath);

        // Assert
        result.IsEmpty().Should().BeTrue("both null values should be considered equal");
        defaultComparer.WasCalled.Should().BeFalse("default comparer should not be called when both values are null");
    }

    [Test]
    public void Compare_WithEmptyPropertyPath_ThrowsArgumentException()
    {
        // Arrange
        var defaultComparer = new TrackingComparer("default");

        var compareValues = new CompareValues(
            defaultComparer,
            new Dictionary<string, ICustomCompareValues>(),
            _ => false,
            new Dictionary<Type, ICustomCompareValues>());

        // Act & Assert
        var act = () => compareValues.Compare("expected", "actual", "");

        act.Should().Throw<ArgumentException>()
            .WithParameterName("propertyPath");
    }

    [Test]
    public void Compare_WithNullPropertyPath_ThrowsArgumentException()
    {
        // Arrange
        var defaultComparer = new TrackingComparer("default");

        var compareValues = new CompareValues(
            defaultComparer,
            new Dictionary<string, ICustomCompareValues>(),
            _ => false,
            new Dictionary<Type, ICustomCompareValues>());

        // Act & Assert
        var act = () => compareValues.Compare("expected", "actual", null!);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("propertyPath");
    }

    /// <summary>
    /// A test comparer that tracks whether it was called.
    /// </summary>
    private class TrackingComparer : ICompareValues, ICustomCompareValues
    {
        private readonly string _name;

        public TrackingComparer(string name)
        {
            _name = name;
        }

        public bool WasCalled { get; private set; }

        public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull
        {
            WasCalled = true;
            return DeepEqualityResult.None();
        }

        DeepEqualityResult ICustomCompareValues.Compare<T>(T expected, T actual, string propertyName)
        {
            WasCalled = true;
            return DeepEqualityResult.None();
        }
    }
}
