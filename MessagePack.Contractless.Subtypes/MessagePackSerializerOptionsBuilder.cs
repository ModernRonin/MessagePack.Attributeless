using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace MessagePack.Contractless.Subtypes
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

        readonly Dictionary<Type, IPropertyToKeyMapping> _propertyMappedTypes =
            new Dictionary<Type, IPropertyToKeyMapping>();

        readonly Dictionary<Type, ISubTypeToKeyMapping> _subTypeMappedTypes =
            new Dictionary<Type, ISubTypeToKeyMapping>();

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

        public MessagePackSerializerOptionsBuilder AutoKeyed<T>() where T : new()
        {
            var formatter = new ConfigurableKeyFormatter<T>();
            formatter.UseAutomaticKeys();
            _propertyMappedTypes.Add(typeof(T), formatter);
            _formatters.Add(formatter);

            return this;
        }

        public MessagePackSerializerOptions Build()
        {
            var formatters = _formatters.ToList();
            if (_doesUseNativeResolvers) formatters.AddRange(_nativeFormatters);
            var composite = CompositeResolver.Create(_formatters.Concat(_nativeFormatters).ToArray(),
                new[] {_options.Resolver});
            return _options.WithResolver(composite);
        }

        public MessagePackSerializerOptionsBuilder SubType<TBase, TSub>() where TSub : TBase, new()
        {
            var formatter = getOrCreate();
            formatter.RegisterSubType<TSub>();
            return _doImplicitlyAutokeySubtypes ? AutoKeyed<TSub>() : this;

            SubTypeFormatter<TBase> getOrCreate()
            {
                var result = _formatters.OfType<SubTypeFormatter<TBase>>().FirstOrDefault();
                if (result != default) return result;
                result = new SubTypeFormatter<TBase>();
                _subTypeMappedTypes.Add(typeof(TBase), result);
                _formatters.Add(result);
                return result;
            }
        }

        public IEnumerable<string> KeyTable
        {
            get
            {
                yield return "---Subtypes---";
                foreach (var (type, mapping) in _subTypeMappedTypes.OrderBy(kvp => kvp.Key.FullName))
                {
                    yield return type.FullName;
                    foreach (var (subtype, key) in mapping.Mappings.OrderBy(kvp => kvp.Key.FullName))
                        yield return $"  - {subtype.FullName} : {key}";
                }

                yield return "---Properties---";
                foreach (var (type, mapping) in _propertyMappedTypes.OrderBy(kvp => kvp.Key.FullName))
                {
                    yield return type.FullName;
                    foreach (var (property, key) in mapping.Mappings.OrderBy(kvp => kvp.Key.Name))
                        yield return $"  - {property.Name} : {key}";
                }
            }
        }
    }
}