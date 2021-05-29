using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack.Formatters;

namespace MessagePack.Attributeless
{
    public class SubTypeMappedFormatterCollection : IEnumerable<KeyValuePair<Type, ISubTypeToKeyMapping>>
    {
        readonly Dictionary<Type, ISubTypeFormatter> _subTypeMappedTypes =
            new Dictionary<Type, ISubTypeFormatter>();

        public IEnumerator<KeyValuePair<Type, ISubTypeToKeyMapping>> GetEnumerator() =>
            _subTypeMappedTypes.OrderBy(kvp => kvp.Key.FullName)
                .Select(kvp => new KeyValuePair<Type, ISubTypeToKeyMapping>(kvp.Key, kvp.Value))
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IMessagePackFormatter Add(Type baseType, Type subType)
        {
            subType.MustBeDefaultConstructable();
            subType.MustBeDerivedFrom(baseType);

            var formatterType = typeof(SubTypeFormatter<>).MakeGenericType(baseType);
            var result = getOrCreate();
            // ReSharper disable once PossibleNullReferenceException
            formatterType.GetMethods()
                .Single(m =>
                    m.Name                   == nameof(SubTypeFormatter<object>.RegisterSubType) &&
                    m.GetParameters().Length == 0)
                .MakeGenericMethod(subType)
                .Invoke(result, Array.Empty<object>());
            return result;

            IMessagePackFormatter getOrCreate()
            {
                var formatter = _subTypeMappedTypes.Values.FirstOrDefault(f => f.GetType() == formatterType);
                if (formatter != default) return formatter;
                formatter = (ISubTypeFormatter) Activator.CreateInstance(formatterType);
                _subTypeMappedTypes.Add(baseType, formatter);
                return formatter;
            }
        }
    }
}