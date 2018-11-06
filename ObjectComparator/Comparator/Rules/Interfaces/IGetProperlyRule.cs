using ObjectsComparator.Comparator.Strategies.Interfaces;

namespace ObjectsComparator.Comparator.Rules.Interfaces
{
    public interface IGetProperlyRule: IGetRule<ICompareValues>, IValidator{}
}