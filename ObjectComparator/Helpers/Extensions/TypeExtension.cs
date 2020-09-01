using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectsComparator.Helpers.Extensions
{
    public static class TypeExtension
    {
        public static bool IsDefault<T>(this T val)
        {
            return EqualityComparer<T>.Default.Equals(val, y: default);
        }

        public static bool IsNotDefault<T>(this T val)
        {
            return !IsDefault(val);
        }

        public static bool ImplementsGenericInterface(this Type type, Type interfaceType)
        {
            return type
                .GetTypeInfo()
                .ImplementedInterfaces
                .Any(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == interfaceType);
        }
    }
}