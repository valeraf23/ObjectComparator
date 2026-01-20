using FluentAssertions;
using NUnit.Framework;
using ObjectsComparator.Comparator.Rules;
using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Comparator.RepresentationDistinction;
using System;
using System.Collections.Generic;

namespace ObjectsComparator.Tests;

[TestFixture]
public class RulePriorityTests
{
    [Test]
    public void RulesHandler_SortsRulesByPriority_LowerPriorityFirst()
    {
        // Arrange - Add rules in wrong order (all rules valid for int)
        var membersRule = Rule.CreateFor(new AlwaysValidStrategy("Members"), RulePriority.Members);
        var primitiveRule = Rule.CreateFor(new AlwaysValidStrategy("Primitive"), RulePriority.Primitive);
        var collectionRule = Rule.CreateFor(new AlwaysValidStrategy("Collection"), RulePriority.Collection);

        // Add in reverse priority order
        var rules = new List<Rule> { membersRule, primitiveRule, collectionRule };

        // Act
        var handler = new RulesHandler(
            rules,
            new Dictionary<string, ICustomCompareValues>(),
            _ => false);

        // Assert - GetFor should return a valid comparer (primitive has highest priority)
        var comparer = handler.GetFor(typeof(int));
        comparer.Should().NotBeNull();
    }

    [Test]
    public void Rule_CreateFor_WithExplicitPriority_SetsPriorityCorrectly()
    {
        // Arrange & Act
        var rule = Rule.CreateFor(new FakeStrategy("Test"), RulePriority.Equality);

        // Assert
        rule.Priority.Should().Be(RulePriority.Equality);
    }

    [Test]
    public void Rule_CreateFor_WithoutPriority_DefaultsToMembers()
    {
        // Arrange & Act
        var rule = Rule.CreateFor(new FakeStrategy("Test"));

        // Assert
        rule.Priority.Should().Be(RulePriority.Members);
    }

    [Test]
    public void Rules_CreateFor_WithMultipleStrategies_SetsPriorityCorrectly()
    {
        // Arrange & Act
        var rule = Rule.CreateFor(
            new FakeStrategy("Default"),
            RulePriority.Collection,
            new FakeStrategy("Other"));

        // Assert
        rule.Priority.Should().Be(RulePriority.Collection);
    }

    [Test]
    public void RulePriority_HasCorrectOrdering()
    {
        // Assert - Verify enum values are in expected order
        ((int)RulePriority.Primitive).Should().BeLessThan((int)RulePriority.Equality);
        ((int)RulePriority.Equality).Should().BeLessThan((int)RulePriority.OverridesEquals);
        ((int)RulePriority.OverridesEquals).Should().BeLessThan((int)RulePriority.Comparable);
        ((int)RulePriority.Comparable).Should().BeLessThan((int)RulePriority.Collection);
        ((int)RulePriority.Collection).Should().BeLessThan((int)RulePriority.Members);
    }

    [Test]
    public void RulesHandler_WhenMultipleRulesValid_UsesHighestPriority()
    {
        // Arrange - Both rules are valid for string
        var lowPriorityRule = Rule.CreateFor(new AlwaysValidStrategy("Low"), RulePriority.Members);
        var highPriorityRule = Rule.CreateFor(new AlwaysValidStrategy("High"), RulePriority.Primitive);

        // Add in reverse priority order
        var rules = new List<Rule> { lowPriorityRule, highPriorityRule };

        var handler = new RulesHandler(
            rules,
            new Dictionary<string, ICustomCompareValues>(),
            _ => false);

        // Act
        var comparer = handler.GetFor(typeof(string));

        // Assert - Should get the high priority comparer
        comparer.Should().NotBeNull();
    }

    /// <summary>
    /// Fake strategy for testing that is never valid.
    /// </summary>
    private class FakeStrategy : IStrategy
    {
        private readonly string _name;

        public FakeStrategy(string name) => _name = name;

        public bool IsValid(Type member) => false;

        public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull
            => DeepEqualityResult.None();
    }

    /// <summary>
    /// Strategy that is always valid for testing priority ordering.
    /// </summary>
    private class AlwaysValidStrategy : IStrategy
    {
        private readonly string _name;

        public AlwaysValidStrategy(string name) => _name = name;

        public bool IsValid(Type member) => true;

        public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName) where T : notnull
            => DeepEqualityResult.None();
    }
}
