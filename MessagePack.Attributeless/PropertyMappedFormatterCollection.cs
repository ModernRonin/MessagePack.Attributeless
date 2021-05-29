using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack.Formatters;

namespace MessagePack.Attributeless
{
    public class PropertyMappedFormatterCollection : IEnumerable<KeyValuePair<Type, IPropertyToKeyMapping>>
    {
        readonly Dictionary<Type, IPropertyToKeyMapping> _propertyMappedTypes =
            new Dictionary<Type, IPropertyToKeyMapping>();

        public IEnumerator<KeyValuePair<Type, IPropertyToKeyMapping>> GetEnumerator() =>
            _propertyMappedTypes.OrderBy(kvp => kvp.Key.FullName).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IMessagePackFormatter Add(Type type)
        {
            type.MustBeDefaultConstructable();

            if (_propertyMappedTypes.ContainsKey(type)) return default;

            var formatterType = typeof(ConfigurableKeyFormatter<>).MakeGenericType(type);
            var result = Activator.CreateInstance(formatterType);
            // ReSharper disable once PossibleNullReferenceException
            formatterType.GetMethod(nameof(ConfigurableKeyFormatter<object>.UseAutomaticKeys))
                .Invoke(result, Array.Empty<object>());
            _propertyMappedTypes.Add(type, (IPropertyToKeyMapping) result);

            return (IMessagePackFormatter) result;
        }
    }
}