using System;

namespace ObjectsComparator.Comparator.Rules.Interfaces
{
    public interface IGetRule<out T>
    {
        T GetFor(Type memberType);
    }
}