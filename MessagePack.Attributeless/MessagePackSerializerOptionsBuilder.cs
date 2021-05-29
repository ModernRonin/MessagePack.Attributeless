using System;
using System.Linq;
using System.Reflection;
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

        readonly Configuration _configuration;

        readonly MessagePackSerializerOptions _options;

        public MessagePackSerializerOptionsBuilder(MessagePackSerializerOptions options,
            bool doImplicitlyAutokeySubtypes)
        {
            _options = options;
            _configuration = new Configuration(doImplicitlyAutokeySubtypes);
            Validation = new Validation(_configuration);
        }

        public MessagePackSerializerOptionsBuilder AddNativeFormatters()
        {
            _configuration.DoesUseNativeResolvers = true;
            return this;
        }

        public MessagePackSerializerOptionsBuilder AllSubTypesOf(Type baseType, params Assembly[] assemblies)
        {
            if (assemblies.Length == 0) assemblies = new[] {baseType.Assembly};

            var subTypes = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && t.IsDerivedFrom(baseType));
            foreach (var subType in subTypes) SubType(baseType, subType);

            return this;
        }

        public MessagePackSerializerOptionsBuilder AllSubTypesOf<TBase>(params Assembly[] assemblies) =>
            AllSubTypesOf(typeof(TBase), assemblies);

        public MessagePackSerializerOptionsBuilder AutoKeyed(Type type)
        {
            _configuration.AddAutoKeyed(type);
            return this;
        }

        public MessagePackSerializerOptionsBuilder AutoKeyed<T>() where T : new() => AutoKeyed(typeof(T));

        public MessagePackSerializerOptions Build()
        {
            var formatters = _configuration.Formatters.ToList();
            if (_configuration.DoesUseNativeResolvers) formatters.AddRange(_nativeFormatters);
            var composite = CompositeResolver.Create(formatters.ToArray(),
                new[] {_options.Resolver});
            return _options.WithResolver(composite);
        }

        public MessagePackSerializerOptionsBuilder GraphOf(Type type)
        {

            return this;
        }

        public MessagePackSerializerOptionsBuilder GraphOf<T>() => GraphOf(typeof(T));

        public MessagePackSerializerOptionsBuilder SubType(Type baseType, Type subType)
        {
            _configuration.AddSubType(baseType, subType);
            return this;
        }

        public MessagePackSerializerOptionsBuilder SubType<TBase, TSub>() where TSub : TBase, new() =>
            SubType(typeof(TBase), typeof(TSub));

        public Validation Validation { get; }
    }
}