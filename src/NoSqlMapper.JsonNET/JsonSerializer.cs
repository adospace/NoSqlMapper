using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NoSqlMapper.JsonNET
{
    public class JsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        internal JsonSerializer()
        {
        }

        internal JsonSerializer(JsonSerializerSettings settings)
        {
            _settings = settings;
        }

        private readonly ConcurrentDictionary<Type, JsonSerializerSettings> _settingsCache = new ConcurrentDictionary<Type, JsonSerializerSettings>();

        public string Serialize<T>(T objectToSerialize, string idPropertyName)
        {
            Validate.NotNull(objectToSerialize, nameof(objectToSerialize));

            var settings = _settings;
            if (idPropertyName != null)
            {
                if (!_settingsCache.TryGetValue(typeof(T), out settings))
                {
                    settings = new JsonSerializerSettings();
                    foreach (var p in settings.GetType().GetProperties())
                        p.SetValue(settings, p.GetValue(_settings));

                    settings.ContractResolver = new ShouldSerializeContractResolver<T>(idPropertyName);

                    _settingsCache.TryAdd(typeof(T), settings);
                }
            }

            return JsonConvert.SerializeObject(objectToSerialize, settings);
        }

        public T Deserialize<T>(string serializedObject)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(serializedObject, nameof(serializedObject));
            return JsonConvert.DeserializeObject<T>(serializedObject, _settings);
        }
    }

    public class ShouldSerializeContractResolver<T> : DefaultContractResolver
    {
        private readonly string _propertyName;

        public ShouldSerializeContractResolver(string propertyName)
        {
            _propertyName = propertyName;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property.DeclaringType == typeof(T) && property.PropertyName == _propertyName)
            {
                property.Ignored = true;
            }
            return property;
        }
    }
}
