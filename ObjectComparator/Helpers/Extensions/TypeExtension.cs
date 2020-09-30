using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ObjectsComparator.Helpers.Extensions
{
    public static class TypeExtension
    {
        private static readonly ConcurrentDictionary<Type, string> TypeNames = new ConcurrentDictionary<Type, string>();

        public static bool IsPrimitiveOrString(this Type type) =>
            type.IsPrimitive || type == typeof(string) || type.IsEnum;

        public static string ToFriendlyTypeName(this Type type)
        {
            if (type == null)
                return "null";

            return TypeNames
                .GetOrAdd(type, _ =>
                {
                    var useShortName = IsPrimitiveOrString(type) || IsAnonymousType(type) ||
                                       type.GetInterfaces().Contains(typeof(IEnumerable));
                    return GetFriendlyTypeName(type, false);
                });
        }

        private static string GetFriendlyTypeName(Type type, bool useFullName)
        {
            const string anonymousTypePrefix = "<>f__";

            var typeName = useFullName ? type.FullName ?? type.Name : type.Name;

            if (!type.GetTypeInfo().IsGenericType) return typeName.Replace(anonymousTypePrefix, string.Empty);

            var genericArgumentNames = type.GetGenericArguments().Select(ga => ga.ToFriendlyTypeName());
            var friendlyGenericName = typeName.Split('`')[0].Replace(anonymousTypePrefix, string.Empty);

            const string anonymousName = "AnonymousType";

            if (friendlyGenericName.StartsWith(anonymousName))
                friendlyGenericName = friendlyGenericName.Remove(anonymousName.Length);

            var joinedGenericArgumentNames = string.Join(", ", genericArgumentNames);

            return $"{friendlyGenericName}<{joinedGenericArgumentNames}>";
        }

        public static bool ImplementsGenericInterface(this Type type, Type interfaceType) =>
            type
                .GetTypeInfo()
                .ImplementedInterfaces
                .Any(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == interfaceType);

        public static bool IsAnonymousType(Type type)
        {
            if (!(type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))) return false;

            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsDefined(typeof(CompilerGeneratedAttribute), false)
                   && typeInfo.IsGenericType
                   && (type.Name.Contains("AnonymousType") || type.Name.Contains("AnonType"))
                   && typeInfo.Attributes.HasFlag(TypeAttributes.NotPublic);
        }

        public static bool IsOverridesEqualsMethod(this Type type)
        {
            var equalsMethod = type.GetMethods().FirstOrDefault(m => m.Name == "Equals" && m.DeclaringType == type);
            if (equalsMethod != null) return equalsMethod.DeclaringType != typeof(object) && !IsAnonymousType(type);

            return false;
        }

        public static bool IsClassAndNotString(this Type type) => !type.IsPrimitive && type != typeof(string) && !type.IsEnum;
    }
}