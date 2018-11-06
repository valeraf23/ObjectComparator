using System;

namespace ObjectsComparator.Comparator.Strategies.Interfaces
{
    public interface IValidator
    {
        bool IsValid(Type member);
    }
}