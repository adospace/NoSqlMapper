using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NoSqlMapper
{
    public class TypeReflector
    {
        private Type _type;
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

        public TypeReflector Navigate(string path)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(path, nameof(path));

            path = path.Trim();
            if (path == ".")
                return this;

            var tokens = path.Split('.');
            var token = tokens.First();
            var arrayBracketIndex = token.IndexOf('[');
            var isArrayToken = arrayBracketIndex > -1;
            if (isArrayToken)
                token = token.Substring(0, arrayBracketIndex);

            var properties = Properties;
            if (properties.TryGetValue(token, out var foundProperty))
            {
                if (tokens.Length > 1)
                    return Navigate(string.Join(".", tokens.Skip(1)));

                return new TypeReflector(foundProperty.PropertyType);
            }

            return null;
        }
    }
}
