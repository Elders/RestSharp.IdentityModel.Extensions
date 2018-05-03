using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RestSharp
{
    public static class SerializerFactory
    {
        static JsonSerializer serializer;

        public static JsonSerializer GetSerializer()
        {
            if (serializer == null)
            {
                var settings = DefaultSettings();
                serializer = JsonSerializer.Create(settings);
            }

            return serializer;
        }

        public static JsonSerializerSettings DefaultSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.Formatting = Formatting.None;
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            settings.ContractResolver = new ContractResolverWithPrivates();

            return settings;
        }
    }

    public class ContractResolverWithPrivates : Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (!prop.Writable)
            {
                var property = member as PropertyInfo;
                if (property != null)
                {
                    var hasPrivateSetter = property.GetSetMethod(true) != null;
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;
        }
    }
}
