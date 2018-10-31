using System;
using ObjectComparator.Comparator.Interfaces;
using ObjectComparator.Comparator.Rules.Interfaces;

namespace ObjectComparator.Comparator.Rules.Implementations
{
    internal class NotSatisfiedRule : IGetRule<ICompareValues>
    {
        public ICompareValues Get(Type memberType) => throw new NotSupportedException($"Not Satisfied Rule for {memberType.FullName}");
    }
}