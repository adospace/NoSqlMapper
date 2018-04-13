using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NoSqlMapper
{
    public class TypeReflector
    {
        private readonly Type _type;

        internal TypeReflector(Type type)
        {
            Validate.NotNull(type, nameof(type));
            _type = type;
        }

        internal static TypeReflector Create<T>()
        {
            return new TypeReflector(typeof(T));
        }

        internal static TypeReflector Create(object value)
        {
            Validate.NotNull(value, nameof(value));
            return new TypeReflector(value.GetType());
        }

        private IDictionary<string, PropertyInfo> _properties;

        public IDictionary<string, PropertyInfo> Properties
        {
            get
            {
                _properties = _properties ?? _type.GetProperties()
                                  .Where(pi => pi.CanRead && pi.CanWrite)
                                  .ToDictionary(_ => _.Name, _ => _, StringComparer.OrdinalIgnoreCase);
                return _properties;
            }
        }

        public bool Is(Type type)
        {
            return _type == type;
        }

        public Type Type => _type;

        public override string ToString()
        {
            return _type.ToString();
        }

        public TypeReflector Navigate(string path)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(path, nameof(path));

            path = path.Trim();
            if (path == ".")
                return this;

            return Navigate(_type, path);
        }



        public static TypeReflector Navigate(Type type, string path)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(path, nameof(path));

            path = path.Trim();
            if (path == ".")
                return new TypeReflector(type);

            var tokens = path.Split('.');
            var token = tokens.First();
            var arrayBracketIndex = token.IndexOf('[');
            var isArrayToken = arrayBracketIndex > -1;
            if (isArrayToken)
                token = token.Substring(0, arrayBracketIndex);

            var properties = type.GetProperties()
                .Where(pi => pi.CanRead && pi.CanWrite)
                .ToDictionary(_ => _.Name, _ => _, StringComparer.OrdinalIgnoreCase);

            if (properties.TryGetValue(token, out var foundProperty))
            {
                var propertyType = foundProperty.PropertyType;

                if (propertyType.IsArray)
                    propertyType = propertyType.GetElementType();
                else if (propertyType.IsGenericType
                         && propertyType.GetInterfaces().Any(x =>
                             x.IsGenericType &&
                             x.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                    propertyType = propertyType.GetGenericArguments()[0];

                if (tokens.Length > 1)
                    return Navigate(propertyType, string.Join(".", tokens.Skip(1)));
                return new TypeReflector(propertyType);
            }

            return null;
        }
    }
}
