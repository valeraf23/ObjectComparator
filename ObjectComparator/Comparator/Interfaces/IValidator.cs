using System;

namespace ObjectsComparator.Comparator.Interfaces
{
    public interface IValidator
    {
        bool IsValid(Type member);
    }
}