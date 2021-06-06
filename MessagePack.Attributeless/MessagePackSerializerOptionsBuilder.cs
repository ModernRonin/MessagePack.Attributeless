using System;
using System.Linq;
using System.Linq.Expressions;
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
            Versioning = new Versioning(_configuration);
        }

        public MessagePackSerializerOptionsBuilder AddNativeFormatters()
        {
            _configuration.DoesUseNativeResolvers = true;
            return this;
        }

        public MessagePackSerializerOptionsBuilder AllSubTypesOf(Type baseType, params Assembly[] assemblies)
        {
            foreach (var subType in baseType.GetSubTypes(assemblies)) SubType(baseType, subType);

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

        public MessagePackSerializerOptionsBuilder GraphOf(Type type, params Assembly[] assemblies)
        {
            var allTypes = type.GetReferencedUserTypes(assemblies).OrderBy(t => t.Name);
            foreach (var t in allTypes)
            {
                if (t.IsAbstract) AllSubTypesOf(t);
                else AutoKeyed(t);
            }

            return this;
        }

        public MessagePackSerializerOptionsBuilder GraphOf<T>() => GraphOf(typeof(T));

        public MessagePackSerializerOptionsBuilder Ignore(Type type) => this;

        public MessagePackSerializerOptionsBuilder Ignore(Type type, PropertyInfo property) =>
            Ignore(type, pi => pi == property);

        public MessagePackSerializerOptionsBuilder Ignore(Type type, Func<PropertyInfo, bool> predicate)
        {
            _configuration.PropertyMappedTypes.Ignore(type, predicate);
            return this;
        }

        public MessagePackSerializerOptionsBuilder Ignore<T>(Func<PropertyInfo, bool> predicate) =>
            Ignore(typeof(T), predicate);

        public MessagePackSerializerOptionsBuilder Ignore<T>() => Ignore(typeof(T));

        public MessagePackSerializerOptionsBuilder
            Ignore<T, TProperty>(Expression<Func<T, TProperty>> accessor) =>
            Ignore(typeof(T), accessor.WriteablePropertyInfo());

        public MessagePackSerializerOptionsBuilder OverrideFormatter(Type targetType, Type formatterType)
        {
            var formatter = Activator.CreateInstance(formatterType);
            _configuration.Override(targetType, (IMessagePackFormatter) formatter);
            return this;
        }

        public MessagePackSerializerOptionsBuilder OverrideFormatter<TTarget, TFormatter>()
            where TFormatter : IMessagePackFormatter<TTarget> =>
            OverrideFormatter(typeof(TTarget), typeof(TFormatter));

        public MessagePackSerializerOptionsBuilder SubType(Type baseType, Type subType)
        {
            _configuration.AddSubType(baseType, subType);
            return this;
        }

        public MessagePackSerializerOptionsBuilder SubType<TBase, TSub>() where TSub : TBase, new() =>
            SubType(typeof(TBase), typeof(TSub));

        public Versioning Versioning { get; }
    }
}