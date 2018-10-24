using System;

namespace ObjectComparator.Helpers.GuardArgument
{
    public static class GuardArgument
    {
        public static void ArgumentIsNotNull<T>(T value) where T : class
        {
            if (value == null) throw new ArgumentNullException();
        }

        public static void ArgumentIsNotNull<T>(T value, string argument) where T : class
        {
            if (value == null) throw new ArgumentNullException(argument);
        }

        public static void ArgumentOutOfCondition<T>(T value, Func<T, bool> condition, string argument)
        {
            if (!condition(value)) throw new ArgumentOutOfRangeException(value.ToString(), argument);
        }

        public static void ArgumentIsNotNull(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"{value} argument should be not NullOrEmpty");
        }
    }
}