using System;
using System.Collections.Concurrent;
using System.Reflection;

#nullable enable

namespace ObjectsComparator.Comparator
{
    internal sealed class PropertyHelper
    {
        private static readonly ConcurrentDictionary<string, Func<object, object>> Cache =
            new ConcurrentDictionary<string, Func<object, object>>();

        private static readonly MethodInfo CallPropertyGetterOpenGenericMethod =
            typeof(PropertyHelper).GetTypeInfo().GetDeclaredMethod(nameof(CallPropertyGetter))!;

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

        public static Func<object, object> MakeFastPropertyGetter(PropertyInfo propertyInfo) =>
            MakeFastPropertyGetter(
                propertyInfo,
                CallPropertyGetterOpenGenericMethod);

        private static Func<object, object> MakeFastPropertyGetter(
            PropertyInfo propertyInfo,
            MethodInfo propertyGetterWrapperMethod)
        {
            var getMethod = propertyInfo.GetMethod!;

            // Create a delegate TDeclaringType -> TValue
            return MakeFastPropertyGetter(
                typeof(Func<,>),
                getMethod,
                propertyGetterWrapperMethod);
        }

        private static Func<object, object> MakeFastPropertyGetter(
            Type openGenericDelegateType,
            MethodInfo propertyGetMethod,
            MethodInfo openGenericWrapperMethod)
        {
            var name = $"{propertyGetMethod.ReflectedType!.FullName}_{propertyGetMethod.Name}";
            return Cache.GetOrAdd(name, (key, property) =>
            {
                var typeInput = propertyGetMethod.DeclaringType!;
                var typeOutput = propertyGetMethod.ReturnType;

                var delegateType = openGenericDelegateType.MakeGenericType(typeInput, typeOutput);
                var propertyGetterDelegate = propertyGetMethod.CreateDelegate(delegateType);

                var wrapperDelegateMethod = openGenericWrapperMethod.MakeGenericMethod(typeInput, typeOutput);
                var accessorDelegate = wrapperDelegateMethod.CreateDelegate(
                    typeof(Func<object, object>),
                    propertyGetterDelegate);

                return (Func<object, object>) accessorDelegate;
            }, propertyGetMethod);
        }

        public static PropertyHelper Instance(PropertyInfo property) => new PropertyHelper(property);

        private static object? CallPropertyGetter<TDeclaringType, TValue>(
            Func<TDeclaringType, TValue> getter,
            object target) =>
            getter((TDeclaringType) target);
    }
}