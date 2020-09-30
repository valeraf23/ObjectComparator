using System;
using System.Collections.Generic;
using System.Linq;
using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Rules
{
    public class RulesHandler
    {
        private readonly Dictionary<string, ICustomCompareValues> _strategies;
        private readonly Func<string, bool> _ignoreStrategy;
        private readonly Rule[] _rules;

        public RulesHandler(Rule[] rules, Dictionary<string, ICustomCompareValues> strategies,
            Func<string, bool> ignoreStrategy)
        {
            _strategies = strategies;
            _ignoreStrategy = ignoreStrategy;
            _rules = rules;
        }

        public CompareValues GetFor(Type memberType)
        {
            var rule = _rules.FirstOrDefault(r => r.IsValid(memberType))?.Get(memberType) ??
                       throw new NotSupportedException($"Not Satisfied Rule for {memberType.FullName}");

            return new CompareValues(rule, _strategies, _ignoreStrategy);
        }
    }
}