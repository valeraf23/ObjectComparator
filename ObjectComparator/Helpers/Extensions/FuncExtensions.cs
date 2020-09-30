using System;
using System.Linq;
using System.Reflection;

namespace ObjectsComparator.Helpers.Extensions
{
    public static class FuncExtensions
    {
        public static FieldInfo? GetTargetField<T, TResult>(
            this Func<T, TResult> func,
            string fieldName)
        {
            return func
                .Target?
                .GetType()
                .GetTypeInfo()
                .DeclaredFields
                .FirstOrDefault(m => m.Name == fieldName);
        }
    }
}