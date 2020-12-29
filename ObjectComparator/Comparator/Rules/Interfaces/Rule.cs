using ObjectsComparator.Comparator.Rules.Implementations;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using System;

namespace ObjectsComparator.Comparator.Rules.Interfaces
{
    public abstract class Rule : IGetRule<ICompareValues>, IValidator
    {
        public abstract ICompareValues Get(Type memberType);

        public abstract bool IsValid(Type member);

        public static Rule CreateFor<T>(T strategy) where T : class, IStrategy => new Rule<T>(strategy);

        public static Rule CreateFor<T>(T defaultRule, params T[] others) where T : class, IStrategy =>
            new Rules<T>(defaultRule, others);
    }
}