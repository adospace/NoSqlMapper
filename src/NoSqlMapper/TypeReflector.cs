using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        private TypeReflector(TypeReflector other)
        {
            _type = other._type;
            _properties = other.Properties;
            IsArray = other.IsArray;
            Name = other.Name;
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

        public bool IsArray { get; private set; }

        private TypeReflector _parent = null;
        public TypeReflector Parent {
            get { return _parent; }
            private set
            {
#if DEBUG
                if (_parent != null || value == null || value.Child != null)
                    throw new InvalidOperationException();
#endif
                _parent = value;
                _parent.Child = this;
            }
        }

        public TypeReflector Child { get; private set; }

        public string Name { get; private set; }

        public string Path => (Parent?.Path == null ? Name : string.Concat(Parent.Path, '.', Name));

        public bool IsObjectArray => IsArray && _type.IsClass && _type != typeof(string);

        public bool IsValueArray => !IsObjectArray;

        public override string ToString()
        {
            return _type.ToString();
        }

        public IEnumerable<TypeReflector> Navigate(string path)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(path, nameof(path));

            return Navigate(_type, path, new TypeReflector(this) );
        }

        public static IEnumerable<TypeReflector> Navigate<T>(string path)
        {
            return Navigate(typeof(T), path);
        }

        public static IEnumerable<TypeReflector> Navigate(Type type, string path)
        {
            return Navigate(type, path, new TypeReflector(type));
        }

        private static IEnumerable<TypeReflector> Navigate(Type type, string path, TypeReflector parentType)
        {
            Validate.NotNullOrEmptyOrWhiteSpace(path, nameof(path));

            path = path.Trim();

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
                var isArray = false;
                if (propertyType.IsArray)
                {
                    propertyType = propertyType.GetElementType();
                    isArray = true;
                }
                else if (propertyType.IsGenericType
                         && propertyType.GetInterfaces().Any(x =>
                             x.IsGenericType &&
                             x.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    propertyType = propertyType.GetGenericArguments()[0];
                    isArray = true;
                }

                var resolvedType = new TypeReflector(propertyType) {IsArray = isArray, Parent = parentType, Name = token};

                yield return resolvedType;

                if (tokens.Length > 1)
                {
                    foreach (var childType in Navigate(propertyType, string.Join(".", tokens.Skip(1)), resolvedType))
                        yield return childType;
                }

            }
        }
    }
}
