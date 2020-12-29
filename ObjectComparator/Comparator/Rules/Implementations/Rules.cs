using ObjectsComparator.Comparator.Strategies.Interfaces;
using ObjectsComparator.Helpers.GuardArgument;
using System;
using System.Linq;

namespace ObjectsComparator.Comparator.Rules.Implementations
{
    public class Rules<T> : Rule<T> where T : class, IStrategy
    {
        public Rules(T defaultRule, params T[] others) : base(defaultRule)
        {
            GuardArgument.ArgumentIsNotNull(others);
            Strategies = others;
        }

        public T[] Strategies { get; }

        public override ICompareValues Get(Type memberType) =>
            Strategies.FirstOrDefault(x => x.IsValid(memberType)) ?? Default;
    }
}