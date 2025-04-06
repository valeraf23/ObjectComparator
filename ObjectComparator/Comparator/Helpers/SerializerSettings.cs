using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ObjectsComparator.Comparator.Helpers;


    internal static class SerializerSettings
    {
        public static readonly JsonSerializerSettings Settings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            },
            Formatting = Formatting.Indented
        };
}
