using System;
using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Rules.Implementations
{
    internal class NotSatisfiedRule : IGetProperlyRule
    {
        public ICompareValues GetFor(Type memberType) =>
            throw new NotSupportedException($"Not Satisfied Rule for {memberType.FullName}");

        public bool IsValid(Type member) => true;
    }
}