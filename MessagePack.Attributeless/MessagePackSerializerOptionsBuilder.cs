using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace MessagePack.Attributeless
{
    public class MessagePackSerializerOptionsBuilder
    {
        static readonly IMessagePackFormatter[] _nativeFormatters =
        {
            NativeDateTimeArrayFormatter.Instance,
            NativeDateTimeFormatter.Instance,
            NativeDecimalFormatter.Instance,
            NativeGuidFormatter.Instance
        };

        readonly bool _doImplicitlyAutokeySubtypes;

        readonly List<IMessagePackFormatter> _formatters = new List<IMessagePackFormatter>();
        readonly MessagePackSerializerOptions _options;

        readonly PropertyMappedFormatterCollection _propertyMappedTypes =
            new PropertyMappedFormatterCollection();

        readonly SubTypeMappedFormatterCollection _subTypeMappedTypes =
            new SubTypeMappedFormatterCollection();

        bool _doesUseNativeResolvers;

        public MessagePackSerializerOptionsBuilder(MessagePackSerializerOptions options,
            bool doImplicitlyAutokeySubtypes)
        {
            _options = options;
            _doImplicitlyAutokeySubtypes = doImplicitlyAutokeySubtypes;
        }

        public MessagePackSerializerOptionsBuilder AddNativeFormatters()
        {
            _doesUseNativeResolvers = true;
            return this;
        }

        public MessagePackSerializerOptionsBuilder AllSubTypesOf<TBase>(params Assembly[] assemblies)
        {
            if (assemblies.Length == 0) assemblies = new[] {typeof(TBase).Assembly};

            return this;
        }

        public MessagePackSerializerOptionsBuilder AutoKeyed(Type type)
        {
            var formatter = _propertyMappedTypes.Add(type);
            _formatters.Add(formatter);

            return this;
        }

        public MessagePackSerializerOptionsBuilder AutoKeyed<T>() where T : new() => AutoKeyed(typeof(T));

        public MessagePackSerializerOptions Build()
        {
            var formatters = _formatters.ToList();
            if (_doesUseNativeResolvers) formatters.AddRange(_nativeFormatters);
            var composite = CompositeResolver.Create(formatters.ToArray(),
                new[] {_options.Resolver});
            return _options.WithResolver(composite);
        }

        public MessagePackSerializerOptionsBuilder SubType(Type baseType, Type subType)
        {
            var formatter = _subTypeMappedTypes.Add(baseType, subType);
            _formatters.Add(formatter);
            return _doImplicitlyAutokeySubtypes ? AutoKeyed(subType) : this;
        }

        public MessagePackSerializerOptionsBuilder SubType<TBase, TSub>() where TSub : TBase, new() =>
            SubType(typeof(TBase), typeof(TSub));

        public byte[] Checksum =>
            new SHA512Managed().ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", KeyTable)));

        public IEnumerable<string> KeyTable
        {
            get
            {
                yield return "---Subtypes---";
                foreach (var (type, mapping) in _subTypeMappedTypes)
                {
                    yield return type.FullName;
                    foreach (var (subtype, key) in mapping.Mappings.OrderBy(kvp => kvp.Key.FullName))
                        yield return $"  - {subtype.FullName} : {key}";
                }

                yield return "---Properties---";
                foreach (var (type, mapping) in _propertyMappedTypes)
                {
                    yield return type.FullName;
                    foreach (var (property, key) in mapping.Mappings.OrderBy(kvp => kvp.Key.Name))
                        yield return $"  - {property.Name} : {key}";
                }
            }
        }
    }
}