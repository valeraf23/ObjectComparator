namespace ObjectsComparator.Comparator.Rules;

/// <summary>
/// Defines the priority order for comparison rules.
/// Lower values have higher priority and are evaluated first.
/// </summary>
public enum RulePriority
{
    /// <summary>
    /// Highest priority - for primitive types (int, string, bool, etc.)
    /// </summary>
    Primitive = 0,

    /// <summary>
    /// For types with equality operator (==) overload
    /// </summary>
    Equality = 100,

    /// <summary>
    /// For types that override Equals() method
    /// </summary>
    OverridesEquals = 200,

    /// <summary>
    /// For types implementing IComparable
    /// </summary>
    Comparable = 300,

    /// <summary>
    /// For collection types (IEnumerable, IDictionary)
    /// </summary>
    Collection = 400,

    /// <summary>
    /// Lowest priority - fallback for complex objects with member-by-member comparison
    /// </summary>
    Members = 500
}
