using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectsComparator.Comparator.Rules
{
    public class CompareValues
    {
        private readonly Dictionary<string, ICustomCompareValues> _strategies;
        private readonly ICompareValues _rule;
        private readonly Func<string, bool> _ignore;

        public CompareValues(ICompareValues rule, Dictionary<string, ICustomCompareValues> strategies,
            Func<string, bool> ignoreStrategy)
        {
            _rule = rule;
            _strategies = strategies;
            _ignore = ignoreStrategy;

        }

        public DeepEqualityResult Compare<T>(T expected, T actual, string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new Exception($"{nameof(propertyName)} should not be null or empty");

            if (_strategies.IsNotEmpty() && _strategies.Any(x => x.Key == propertyName))
                return _strategies[propertyName].Compare(expected, actual, propertyName);

            if (expected is null && actual is not null)
                return DeepEqualityResult.Create(propertyName, "null", actual);

            if (expected is not null && actual is null)
                return DeepEqualityResult.Create(propertyName, expected, "null");

            if (expected is null)
                return DeepEqualityResult.None();

            return _ignore(propertyName) ? DeepEqualityResult.None() : _rule.Compare(expected, actual, propertyName);
        }
    }
}