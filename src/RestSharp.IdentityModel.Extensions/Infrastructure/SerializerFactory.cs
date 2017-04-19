using Newtonsoft.Json;

namespace RestSharp.IdentityModel.Extensions.Infrastructure
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
            settings.Formatting = Formatting.Indented;
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            var contractReslover = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            contractReslover.DefaultMembersSearchFlags = contractReslover.DefaultMembersSearchFlags | System.Reflection.BindingFlags.NonPublic;
            settings.ContractResolver = contractReslover;

            return settings;
        }
    }
}
