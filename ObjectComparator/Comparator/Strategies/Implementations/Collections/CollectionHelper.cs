using System;
using System.Reflection;
using ObjectsComparator.Comparator.RepresentationDistinction;
using ObjectsComparator.Comparator.Rules;

namespace ObjectsComparator.Comparator.Strategies.Implementations.Collections
{
    internal class CollectionHelper
    {
        private static readonly MethodInfo CallWrapper =
            typeof(CollectionHelper).GetTypeInfo().GetDeclaredMethod(nameof(Wrapper))!;

        private static readonly Type RulesHandlerType = typeof(RulesHandler);
        private static readonly Type DistinctionsType = typeof(DeepEqualityResult);
        private static readonly Type StringType = typeof(string);

        private static DeepEqualityResult Wrapper<TTarget>(Func<TTarget, TTarget, string, RulesHandler, DeepEqualityResult> func,
            object expected, object actual, string propertyName, RulesHandler comparator)
        {
            var ex = (TTarget) expected;
            var act = (TTarget) actual;
            return func(ex, act, propertyName, comparator);
        }

        public static Func<object, object, string, RulesHandler, DeepEqualityResult> GetDelegateFor(MethodInfo method)
        {
            var targetType = method.GetParameters()[0].ParameterType;
            var delegateType = typeof(Func<,,,,>).MakeGenericType(targetType, targetType, StringType, RulesHandlerType,
                DistinctionsType);

            var delegateCompareIListTypes = method.CreateDelegate(delegateType);
            return (Func<object, object, string, RulesHandler, DeepEqualityResult>) CallWrapper
                .MakeGenericMethod(targetType).CreateDelegate(
                    typeof(Func<object, object, string, RulesHandler, DeepEqualityResult>),
                    delegateCompareIListTypes);
        }
    }
}