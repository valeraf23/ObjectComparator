using System;

namespace ObjectComparator.Comparator.Interfaces
{
    public interface IValidator
    {
        bool IsValid(Type member);
    }
}