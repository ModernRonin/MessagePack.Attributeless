using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MessagePack.Attributeless.Implementation;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace MessagePack.Attributeless
{
    /// <summary>
    ///     Fluent builder returned by <see cref="MessagePackSerializerOptionsExtensions.Configure" />.
    /// </summary>
    public sealed class MessagePackSerializerOptionsBuilder
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

        /// <summary>
        ///     Adds all C# native formatters to the configuration which have better performance than their non-native
        ///     counterparts.
        ///     <para>Use this only if you know that you will only ever serialize and deserialize from C#.</para>
        /// </summary>
        public MessagePackSerializerOptionsBuilder AddNativeFormatters()
        {
            _configuration.DoesUseNativeResolvers = true;
            return this;
        }

        /// <summary>
        ///     Non-generic version of <see cref="AllSubTypesOf{TBase}" /> for when you need to discover the types dynamically.
        /// </summary>
        public MessagePackSerializerOptionsBuilder AllSubTypesOf(Type baseType, params Assembly[] assemblies)
        {
            foreach (var subType in baseType.GetSubTypes(assemblies)) SubType(baseType, subType);

            return this;
        }

        /// <summary>
        ///     Register all concrete implementations/derivations of
        ///     <typeparamref name="TBase" />
        ///     found in
        ///     <paramref name="assemblies" />.
        ///     <para>Only public types will be considered and types must be default-constructable.</para>
        ///     <para>
        ///         If all your implementations reside in the same assembly as
        ///         <typeparamref name="TBase" />
        ///         you don't need to pass
        ///         any assembly, that assembly will be considered implicitly. Vice versa, if you do pass assemblies, then you have
        ///         to add the assembly of
        ///         <typeparamref name="TBase" />
        ///         explicitly.
        ///     </para>
        /// </summary>
        public MessagePackSerializerOptionsBuilder AllSubTypesOf<TBase>(params Assembly[] assemblies) =>
            AllSubTypesOf(typeof(TBase), assemblies);

        /// <summary>
        ///     Non-generic version of <see cref="AutoKeyed{T}" /> for when you need to discover the types dynamically.
        /// </summary>
        public MessagePackSerializerOptionsBuilder AutoKeyed(Type type)
        {
            _configuration.AddAutoKeyed(type);
            return this;
        }

        /// <summary>
        ///     Register the type
        ///     <typeparamref name="T" />
        ///     as auto-keyed, ie, all properties will be stored via auto-generated
        ///     keys.
        ///     <para>
        ///         Indexer properties will be ignored, ie, they will neither be serialized nor create any problems for the
        ///         serialization or deserialization of the other properties.
        ///     </para>
        /// </summary>
        public MessagePackSerializerOptionsBuilder AutoKeyed<T>() where T : new() => AutoKeyed(typeof(T));

        /// <summary>
        ///     Ends the fluent configuration and returns a configured <see cref="MessagePackSerializerOptions" /> instance you can
        ///     use.
        /// </summary>
        public MessagePackSerializerOptions Build()
        {
            var formatters = _configuration.Formatters.ToList();
            if (_configuration.DoesUseNativeResolvers) formatters.AddRange(_nativeFormatters);
            var composite = CompositeResolver.Create(formatters.ToArray(),
                new[] {_options.Resolver});
            return _options.WithResolver(composite);
        }

        /// <summary>
        ///     Non-generic version of <see cref="GraphOf{T}" /> for when you need to discover the types dynamically.
        /// </summary>
        public MessagePackSerializerOptionsBuilder GraphOf(Type type, params Assembly[] assemblies)
        {
            var allTypes = type.GetReferencedUserTypes(assemblies).OrderBy(t => t.Name);
            foreach (var t in allTypes)
            {
                if (t.IsAbstract) AllSubTypesOf(t, assemblies);
                else AutoKeyed(t);
            }

            return this;
        }

        /// <summary>
        ///     Automatically create configuration for the whole type graph rooted in
        ///     <typeparamref name="T" />
        ///     .
        ///     <para>This works by looking at any public properties and their types and configuring them all, recursively.</para>
        ///     <para>
        ///         Properties are implicitly auto-keyed - same as if you called <see cref="AutoKeyed{T}" /> explicitly - and
        ///         sub-types are considered in the same manner as if you called <see cref="AllSubTypesOf{TBase}" />.
        ///     </para>
        ///     <para>
        ///         If you want to ignore certain properties, follow up by a call to one of the overloads of
        ///         <see cref="Ignore{T}" />.
        ///     </para>
        ///     <para>
        ///         If you want to customize the behavior for one specific type, follow up by a call to
        ///         <see cref="OverrideFormatter{TTarget,TFormatter}" />.
        ///     </para>
        ///     <para>
        ///         If this is still not enough, you have to fall back to the more explicit configuration via
        ///         <see cref="AutoKeyed{T}" /> and <see cref="AllSubTypesOf{TBase}" /> or <see cref="SubType{TBase,TSub}" />.
        ///     </para>
        /// </summary>
        public MessagePackSerializerOptionsBuilder GraphOf<T>(params Assembly[] assemblies) =>
            GraphOf(typeof(T), assemblies);

        /// <summary>
        ///     Non-generic version of
        ///     <see cref="Ignore{T,TProperty}" /> for when you need to discover the types dynamically.
        /// </summary>
        public MessagePackSerializerOptionsBuilder Ignore(Type type, PropertyInfo property) =>
            Ignore(type, pi => pi == property);

        /// <summary>
        ///     Non-generic version of
        ///     <see cref="Ignore{T}(Func{PropertyInfo,bool})" /> for when you need to discover the types dynamically.
        /// </summary>
        public MessagePackSerializerOptionsBuilder Ignore(Type type, Func<PropertyInfo, bool> predicate)
        {
            _configuration.PropertyMappedTypes.Ignore(type, predicate);
            return this;
        }

        /// <summary>
        ///     Configures the serializer to ignore all properties of <typeparamref name="T" /> for which
        ///     <paramref name="predicate" />
        ///     returns true.
        /// </summary>
        public MessagePackSerializerOptionsBuilder Ignore<T>(Func<PropertyInfo, bool> predicate) =>
            Ignore(typeof(T), predicate);

        /// <summary>
        ///     Configures the serializer to ignore the property <paramref name="accessor" /> of <typeparamref name="T" />.
        /// </summary>
        public MessagePackSerializerOptionsBuilder
            Ignore<T, TProperty>(Expression<Func<T, TProperty>> accessor) =>
            Ignore(typeof(T), accessor.WriteablePropertyInfo());

        /// <summary>
        ///     Non-generic version of <see cref="OverrideFormatter{TTarget,TFormatter}" /> for when you need to discover the types
        ///     dynamically.
        /// </summary>
        public MessagePackSerializerOptionsBuilder OverrideFormatter(Type targetType, Type formatterType)
        {
            var formatter = Activator.CreateInstance(formatterType);
            _configuration.Override(targetType, (IMessagePackFormatter) formatter);
            return this;
        }

        /// <summary>
        ///     Configures the serializer to use <typeparamref name="TFormatter" /> for serializing/deserializing
        ///     <typeparamref name="TTarget " />.
        /// </summary>
        public MessagePackSerializerOptionsBuilder OverrideFormatter<TTarget, TFormatter>()
            where TFormatter : IMessagePackFormatter<TTarget> =>
            OverrideFormatter(typeof(TTarget), typeof(TFormatter));

        /// <summary>
        ///     Non-generic version of <see cref="SubType{TBase,TSub}" /> for when you need to discover the types dynamically.
        /// </summary>
        public MessagePackSerializerOptionsBuilder SubType(Type baseType, Type subType)
        {
            _configuration.AddSubType(baseType, subType);
            return this;
        }

        /// <summary>
        ///     Registers <typeparamref name="TSub" /> as a sub-type of <typeparamref name="TBase" />.
        ///     <para>Use this if the behavior of <see cref="AllSubTypesOf{TBase}" /> does not fit your requirements.</para>
        /// </summary>
        public MessagePackSerializerOptionsBuilder SubType<TBase, TSub>() where TSub : TBase, new() =>
            SubType(typeof(TBase), typeof(TSub));

        /// <summary>
        ///     Returns information you can use to detect changes of the configuration, for example if you rename types or
        ///     properties.
        /// </summary>
        public Versioning Versioning { get; }
    }
}