using System.Collections.Generic;

namespace ObjectsComparator.Comparator.Strategies.StrategiesForCertainProperties
{
    internal static class ReplaceValuesDictionary
    {
        static ReplaceValuesDictionary()
        {
            IDictionary<string, string> listValues = new Dictionary<string, string>
            {
                [".get_Item"] = "",
                ["("] = "[",
                [")"] = "]"
            };

            Swaps = new[] {listValues};
        }

        private static readonly IList<IDictionary<string, string>> Swaps;

        public static string Replace(string str)
        {
            foreach (var sw in Swaps)
            {
                foreach (var (oldValue, newValue) in sw)
                {
                    str = str.Replace(oldValue, newValue);
                }
            }

            return str;
        }
    }
}