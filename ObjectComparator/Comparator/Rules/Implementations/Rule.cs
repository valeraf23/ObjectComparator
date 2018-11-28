using System;
using System.Collections.Generic;
using System.Linq;
using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.Extensions;
using ObjectsComparator.Helpers.GuardArgument;

namespace ObjectsComparator.Comparator.Rules.Implementations
{
    public class Rule<T> : IGetProperlyRule where T : class, IStrategy
    {
        public Rule(T defaultRule)
        {
            GuardArgument.ArgumentIsNotNull(defaultRule);
            Default = defaultRule;
            Strategies = new Stack<T>();
            Strategies.Push(defaultRule);
        }

        public Rule(T defaultRule, IList<T> others) : this(defaultRule)
        {
            GuardArgument.ArgumentIsNotNull(others);
            others.ForEach(x => Strategies.Push(x));
        }

        public T Default { get; }

        public Stack<T> Strategies { get; }

        public ICompareValues GetFor(Type memberType)
        {
            return Strategies.FirstOrDefault(x => x.IsValid(memberType)) ?? Default;
        }

        public bool IsValid(Type member)
        {
            return Default.IsValid(member);
        }

        public Rule<T> Add(T newRule)
        {
            Strategies.Push(newRule);
            return this;
        }

        public Rule<T> AddRange(IEnumerable<T> newRules)
        {
            newRules.ForEach(x => Strategies.Push(x));
            return this;
        }
    }
}