using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Strategies.Interfaces;
using System;

namespace ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties
{
    /// <summary>
    /// A strategy for comparing values based on their runtime type using a custom comparison function.
    /// </summary>
    internal sealed class TypeMemberStrategy : ICustomCompareValues
    {
        private readonly Func<object?, object?, bool> _comparer;
        private readonly Display _display;

        public TypeMemberStrategy(Func<object?, object?, bool> comparer, Display display)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            _display = display ?? new Display();
        }

        public DeepEqualityResult Compare<T>(T expected, T actual, string propertyPath)
        {
            try
            {
                var isEqual = _comparer(expected, actual);
                if (isEqual)
                {
                    return DeepEqualityResult.None();
                }

                var distinction = _display.GetDistinction(expected, actual, propertyPath, "Type-based custom comparison");
                return DeepEqualityResult.Create(distinction);
            }
            catch (Exception ex)
            {
                return DeepEqualityResult.Create(new Distinction(propertyPath, expected, actual, 
                    $"Type strategy comparison failed: {ex.Message}"));
            }
        }
    }
}
