using System;
using ObjectsComparator.Comparator.Interfaces;
using ObjectsComparator.Comparator.Rules.Interfaces;

namespace ObjectsComparator.Comparator.Rules.Implementations
{
    internal class NotSatisfiedRule : IGetRule<ICompareValues>
    {
        public ICompareValues Get(Type memberType) => throw new NotSupportedException($"Not Satisfied Rule for {memberType.FullName}");
    }
}