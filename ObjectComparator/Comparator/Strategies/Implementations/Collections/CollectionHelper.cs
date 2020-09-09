using System;
using System.Reflection;
using ObjectsComparator.Comparator.RepresentationDistinction;

#nullable enable
namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections
{
    internal class CollectionHelper
    {
        private static readonly MethodInfo CallWrapper =
            typeof(CollectionHelper).GetTypeInfo().GetDeclaredMethod(nameof(Wrapper))!;

        private static Distinctions Wrapper<TTarget>(Func<TTarget, TTarget, string, Comparator, Distinctions> func,
            object expected, object actual, string propertyName, Comparator comparator)
        {
            var ex = (TTarget) expected;
            var act = (TTarget) actual;
            return func(ex, act, propertyName, comparator);
        }

        public static Func<object, object, string, Comparator, Distinctions> GetDelegateFor(MethodInfo method)
        {
            var targetType = method.GetParameters()[0].ParameterType;
            var delegateType = typeof(Func<,,,,>).MakeGenericType(targetType, targetType, typeof(string),
                typeof(Comparator),
                typeof(Distinctions));

            var delegateCompareIListTypes = method.CreateDelegate(delegateType);
            return (Func<object, object, string, Comparator, Distinctions>) CallWrapper
                .MakeGenericMethod(targetType).CreateDelegate(
                    typeof(Func<object, object, string, Comparator, Distinctions>),
                    delegateCompareIListTypes);
        }
    }
}