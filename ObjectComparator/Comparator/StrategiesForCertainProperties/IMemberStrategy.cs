using ObjectComparator.Comparator.Interfaces;

namespace ObjectComparator.Comparator.StrategiesForCertainProperties
{
    public interface IMemberStrategy : ICompareValues
    {
        string MemberName { get; set; }
    }
}