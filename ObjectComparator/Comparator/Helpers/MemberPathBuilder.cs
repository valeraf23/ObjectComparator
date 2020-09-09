using System.Reflection;

namespace ObjectsComparator.Comparator.Helpers
{
    public static class MemberPathBuilder
    {
        public static string BuildMemberPath(string parentMemberName, MemberInfo mi)
        {
            return BuildMemberPath(parentMemberName, mi.Name);
        }

        public static string BuildMemberPath(string parentMemberName, string name)
        {
            var memberName = name;
            return !string.IsNullOrEmpty(parentMemberName) ? $"{parentMemberName}.{memberName}" : memberName;
        }
    }
}