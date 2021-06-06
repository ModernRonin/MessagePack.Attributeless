using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MessagePack.Attributeless.Formatters;

namespace MessagePack.Attributeless.Implementation
{
    public sealed class PropertyMappedFormatterCollection
        : IEnumerable<KeyValuePair<Type, IPropertyFormatter>>
    {
        readonly Dictionary<Type, IPropertyFormatter> _propertyMappedTypes =
            new Dictionary<Type, IPropertyFormatter>();

        public IEnumerator<KeyValuePair<Type, IPropertyFormatter>> GetEnumerator() =>
            _propertyMappedTypes.OrderBy(kvp => kvp.Key.FullName).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(Type type)
        {
            type.MustBeDefaultConstructable();

            if (_propertyMappedTypes.ContainsKey(type)) return;

            var formatterType = typeof(ConfigurableKeyFormatter<>).MakeGenericType(type);
            var result = Activator.CreateInstance(formatterType);
            // ReSharper disable once PossibleNullReferenceException
            formatterType.GetMethod(nameof(ConfigurableKeyFormatter<object>.UseAutomaticKeys))
                .Invoke(result, Array.Empty<object>());
            _propertyMappedTypes.Add(type, (IPropertyFormatter) result);
        }

        public void Ignore(Type type, Func<PropertyInfo, bool> predicate)
        {
            if (!_propertyMappedTypes.ContainsKey(type))
            {
                throw new InvalidOperationException(
                    $"The type {type.Name} is not registered. Add Ignore clauses after registering types.");
            }

            _propertyMappedTypes[type].Ignore(predicate);
        }
    }
}