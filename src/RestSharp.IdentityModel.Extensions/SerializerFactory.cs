using System;
using System.Collections.Generic;
using System.Linq;
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
            settings.Formatting = Formatting.Indented;
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;

#if NETSTANDARD2_0
            settings.ContractResolver = new ContractResolverWithPrivates();
#else
            var contractReslover = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
            contractReslover.DefaultMembersSearchFlags = contractReslover.DefaultMembersSearchFlags | System.Reflection.BindingFlags.NonPublic;
            settings.ContractResolver = contractReslover;
#endif
            return settings;
        }
    }

#if NETSTANDARD2_0
    public class ContractResolverWithPrivates : Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = type
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(p => base.CreateProperty(p, memberSerialization))
                .Union(type
                    .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Select(f => base.CreateProperty(f, memberSerialization)))
                .ToList();
            props.ForEach(p => { p.Writable = true; p.Readable = true; });
            return props;
        }

        override protected List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var props = objectType
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Select(x => (MemberInfo)x)
                .Union(objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                .ToList();
            return props;
        }
    }
#endif
}
