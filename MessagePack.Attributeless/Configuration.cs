using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack.Formatters;

namespace MessagePack.Attributeless
{
    public sealed class Configuration
    {
        readonly bool _doImplicitlyAutokeySubtypes;

        readonly Dictionary<Type, IMessagePackFormatter> _overrides =
            new Dictionary<Type, IMessagePackFormatter>();

        public Configuration(bool doImplicitlyAutokeySubtypes) =>
            _doImplicitlyAutokeySubtypes = doImplicitlyAutokeySubtypes;

        public void AddAutoKeyed(Type type) => PropertyMappedTypes.Add(type);

        public void AddSubType(Type baseType, Type subType)
        {
            SubTypeMappedTypes.Add(baseType, subType);
            if (_doImplicitlyAutokeySubtypes) AddAutoKeyed(subType);
        }

        public void Override(Type type, IMessagePackFormatter formatter) => _overrides[type] = formatter;

        public bool DoesUseNativeResolvers { get; set; }

        public IEnumerable<IMessagePackFormatter> Formatters
        {
            get
            {
                foreach (var (type, formatter) in SubTypeMappedTypes)
                    if (!_overrides.ContainsKey(type))
                        yield return formatter;

                foreach (var (type, formatter) in PropertyMappedTypes)
                    if (!_overrides.ContainsKey(type))
                        yield return formatter;

                foreach (var (_, formatter) in _overrides) yield return formatter;
            }
        }

        public IReadOnlyDictionary<Type, Type> Overrides =>
            _overrides.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetType());

        public PropertyMappedFormatterCollection PropertyMappedTypes { get; } =
            new PropertyMappedFormatterCollection();

        public SubTypeMappedFormatterCollection SubTypeMappedTypes { get; } =
            new SubTypeMappedFormatterCollection();
    }
}