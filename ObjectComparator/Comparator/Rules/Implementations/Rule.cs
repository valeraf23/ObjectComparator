using System;
using System.Collections.Generic;
using System.Linq;
using ObjectComparator.Comparator.Interfaces;
using ObjectComparator.Comparator.Rules.Interfaces;
using ObjectComparator.Helpers.GuardArgument;

namespace ObjectComparator.Comparator.Rules.Implementations
{
    public class Rule<T> : IGetProperlyRule where T : class, IStrategy
    {
        public Rule(T defaultRule)
        {
            GuardArgument.ArgumentIsNotNull(defaultRule);

            Default = defaultRule;
            Others = new List<T>();
        }

        public Rule(T defaultRule, IList<T> others) : this(defaultRule)
        {
            GuardArgument.ArgumentIsNotNull(others);

            Default = defaultRule;
            Others = others.ToList();
        }

        public Rule<T> Add(T newRule)
        {
            Others.Add(newRule);
            return this;
        }

        public Rule<T> AddRange(IEnumerable<T> newRules)
        {
            Others.AddRange(newRules);
            return this;
        }

        public T Default { get; }
        public List<T> Others { get; }
        public ICompareValues Get<T1>() => Others.FirstOrDefault(x => x.IsValid(typeof(T1))) ?? Default;
        public bool IsValid(Type member) => Default.IsValid(member);
    }
}