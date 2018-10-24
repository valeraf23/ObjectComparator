namespace ObjectComparator.Comparator.Rules.Interfaces
{
    public interface IGetRule<out T>
    {
        T Get<T1>();
    }
}