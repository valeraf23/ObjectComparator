using System;
using System.Collections.Concurrent;
using System.Reflection;

#nullable enable
namespace ObjectsComparator.Helpers
{
    internal sealed class PropertyHelper
    {
        private delegate TValue ByRefFunc<TDeclaringType, out TValue>(ref TDeclaringType arg);

        private static readonly ConcurrentDictionary<string, Func<object, object>> Cache = new();

        private static readonly MethodInfo CallPropertyGetterOpenGenericMethod =
            typeof(PropertyHelper).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertyGetter))!;

        private static readonly MethodInfo CallPropertyGetterByReferenceOpenGenericMethod =
            typeof(PropertyHelper).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertyGetterByReference))!;

        private Func<object, object>? _valueGetter;

        private PropertyHelper(PropertyInfo property)
        {
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Name = property.Name;
        }

        public PropertyInfo Property { get; }

        public string Name { get; }

        public Func<object, object> ValueGetter => _valueGetter ??= MakeFastPropertyGetter(Property);

        public object GetValue(object instance) => ValueGetter(instance);

        private static Func<object, object> MakeFastPropertyGetter(
            PropertyInfo propertyInfo)
        {
            var getMethod = propertyInfo.GetMethod!;

            if (getMethod.DeclaringType!.GetTypeInfo().IsValueType)
            {
                // Create a delegate (ref TDeclaringType) -> TValue
                return MakeFastPropertyGetter(
                    typeof(ByRefFunc<,>),
                    getMethod,
                    CallPropertyGetterByReferenceOpenGenericMethod);
            }

            // Create a delegate TDeclaringType -> TValue
            return MakeFastPropertyGetter(
                typeof(Func<,>),
                getMethod,
                CallPropertyGetterOpenGenericMethod);
        }

        private static Func<object, object> MakeFastPropertyGetter(
            Type openGenericDelegateType,
            MethodInfo propertyGetMethod,
            MethodInfo openGenericWrapperMethod)
        {
            if (openGenericDelegateType is null) throw new ArgumentNullException(nameof(openGenericDelegateType));
            var name = $"{propertyGetMethod.ReflectedType!.FullName}_{propertyGetMethod.Name}";
            return Cache.GetOrAdd(name, (_, _) =>
            {
                var typeInput = propertyGetMethod.DeclaringType!;
                var typeOutput = propertyGetMethod.ReturnType;

                var delegateType = openGenericDelegateType.MakeGenericType(typeInput, typeOutput);
                var propertyGetterDelegate = propertyGetMethod.CreateDelegate(delegateType);

                var wrapperDelegateMethod = openGenericWrapperMethod.MakeGenericMethod(typeInput, typeOutput);
                var accessorDelegate = wrapperDelegateMethod.CreateDelegate(
                    typeof(Func<object, object>),
                    propertyGetterDelegate);

                return (Func<object, object>)accessorDelegate;
            }, propertyGetMethod);
        }

        public static PropertyHelper Instance(PropertyInfo property) => new(property);

        private static object? CallPropertyGetter<TDeclaringType, TValue>(
            Func<TDeclaringType, TValue> getter,
            object target) => getter((TDeclaringType)target);

        private static object? CallPropertyGetterByReference<TDeclaringType, TValue>(
            ByRefFunc<TDeclaringType, TValue> getter,
            object target)
        {
            var unboxed = (TDeclaringType)target;
            return getter(ref unboxed);
        }
    }
}