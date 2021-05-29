using System;
using System.Collections.Generic;
using MessagePack.Formatters;

namespace MessagePack.Attributeless
{
    public class Configuration
    {
        readonly bool _doImplicitlyAutokeySubtypes;

        public Configuration(bool doImplicitlyAutokeySubtypes) =>
            _doImplicitlyAutokeySubtypes = doImplicitlyAutokeySubtypes;

        public void AddAutoKeyed(Type type)
        {
            var formatter = PropertyMappedTypes.Add(type);
            Formatters.Add(formatter);
        }

        public void AddSubType(Type baseType, Type subType)
        {
            var formatter = SubTypeMappedTypes.Add(baseType, subType);
            Formatters.Add(formatter);
            if (_doImplicitlyAutokeySubtypes) AddAutoKeyed(subType);
        }

        public bool DoesUseNativeResolvers { get; set; }
        public List<IMessagePackFormatter> Formatters { get; } = new List<IMessagePackFormatter>();

        public PropertyMappedFormatterCollection PropertyMappedTypes { get; } =
            new PropertyMappedFormatterCollection();

        public SubTypeMappedFormatterCollection SubTypeMappedTypes { get; } =
            new SubTypeMappedFormatterCollection();
    }
}