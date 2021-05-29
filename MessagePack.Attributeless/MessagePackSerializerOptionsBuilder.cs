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

        public MessagePackSerializerOptionsBuilder AllSubTypesOf<TBase>(params Assembly[] assemblies)
        {
            if (assemblies.Length == 0) assemblies = new[] {typeof(TBase).Assembly};

            return this;
        }

        public MessagePackSerializerOptionsBuilder AutoKeyed(Type type)
        {
            type.MustBeDefaultConstructable();

            var formatterType = typeof(ConfigurableKeyFormatter<>).MakeGenericType(type);
            var formatter = Activator.CreateInstance(formatterType);
            // ReSharper disable once PossibleNullReferenceException
            formatterType.GetMethod(nameof(ConfigurableKeyFormatter<object>.UseAutomaticKeys))
                .Invoke(formatter, Array.Empty<object>());
            _propertyMappedTypes.Add(type, (IPropertyToKeyMapping) formatter);
            _formatters.Add((IMessagePackFormatter) formatter);

            return this;
        }

        public MessagePackSerializerOptionsBuilder AutoKeyed<T>() where T : new() => AutoKeyed(typeof(T));

        public MessagePackSerializerOptions Build()
        {
            var formatters = _formatters.ToList();
            if (_doesUseNativeResolvers) formatters.AddRange(_nativeFormatters);
            var composite = CompositeResolver.Create(_formatters.Concat(_nativeFormatters).ToArray(),
                new[] {_options.Resolver});
            return _options.WithResolver(composite);
        }

        public MessagePackSerializerOptionsBuilder SubType(Type baseType, Type subType)
        {
            subType.MustBeDefaultConstructable();
            subType.MustBeDerivedFrom(baseType);

            var formatterType = typeof(SubTypeFormatter<>).MakeGenericType(baseType);
            var formatter = getOrCreate();
            // ReSharper disable once PossibleNullReferenceException
            formatterType.GetMethods()
                .Single(m =>
                    m.Name                   == nameof(SubTypeFormatter<object>.RegisterSubType) &&
                    m.GetParameters().Length == 0)
                .MakeGenericMethod(subType)
                .Invoke(formatter, Array.Empty<object>());
            return _doImplicitlyAutokeySubtypes ? AutoKeyed(subType) : this;

            IMessagePackFormatter getOrCreate()
            {
                var result = _formatters.FirstOrDefault(f => f.GetType() == formatterType);
                if (result != default) return result;
                result = (IMessagePackFormatter) Activator.CreateInstance(formatterType);
                _subTypeMappedTypes.Add(baseType, (ISubTypeToKeyMapping) result);
                _formatters.Add(result);
                return result;
            }
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