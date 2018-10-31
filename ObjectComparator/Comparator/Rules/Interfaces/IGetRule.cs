using System;

namespace ObjectComparator.Comparator.Rules.Interfaces
{
    public interface IGetRule<out T>
    {
        T Get(Type memberType);
    }
}