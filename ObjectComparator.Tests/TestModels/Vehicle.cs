using System.Collections.Generic;

namespace ObjectsComparator.Tests.TestModels
{
    internal class Vehicle
    {
        public string Model { get; set; }
    }
    public class A
    {
        public List<B> B { get; set; }
    }

    public class B
    {
        public string C { get; set; }
    }

    public static class Ignore
    {
        public static bool IgnoreProperty(string propertyPath)
        {
            var ignoreProperties = new[] {
                "C"
            };

            foreach (var ignoreProperty in ignoreProperties)
            {
                if (propertyPath.EndsWith(ignoreProperty))
                {
                    return true;
                }
            }

            return false;
        }
    }
}