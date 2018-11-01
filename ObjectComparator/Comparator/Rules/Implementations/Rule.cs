using System;
using System.Collections.Generic;
using System.Linq;
using ObjectsComparator.Comparator.Interfaces;
using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Helpers.GuardArgument;

namespace ObjectsComparator.Comparator.Rules.Implementations
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
        public ICompareValues Get(Type memberType) => Others.FirstOrDefault(x => x.IsValid(memberType)) ?? Default;
        public bool IsValid(Type member) => Default.IsValid(member);
    }
}