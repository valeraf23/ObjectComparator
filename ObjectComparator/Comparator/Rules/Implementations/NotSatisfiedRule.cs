using System;
using ObjectsComparator.Comparator.Rules.Interfaces;
using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Rules.Implementations
{
    internal class NotSatisfiedRule : IGetRule<ICompareValues>
    {
        public ICompareValues Get(Type memberType) => throw new NotSupportedException($"Not Satisfied Rule for {memberType.FullName}");
    }
}