using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using MessagePack.Formatters;

namespace MessagePack.Attributeless.Implementation
{
    public static class TypeExtensions
    {
        static readonly ImmutableHashSet<Type> _builtinMessagePackTypes;

        static TypeExtensions()
        {
            _builtinMessagePackTypes = typeof(BooleanArrayFormatter).Assembly.GetExportedTypes()
                .Where(isFormatter)
                .Select(targetType)
                .ToImmutableHashSet();

            bool isFormatter(Type t) =>
                !t.IsAbstract &&
                t.GetInterfaces().Any(i => i.Name == typeof(IMessagePackFormatter<>).Name);

            Type targetType(Type t)
            {
                var typeArg = t.GetInterface(typeof(IMessagePackFormatter<>).Name)
                    .GenericTypeArguments.Single();
                return typeArg.IsGenericType ? typeArg.GetGenericTypeDefinition() : typeArg;
            }
        }

        public static IEnumerable<Type> GetReferencedUserTypes(this Type self, params Assembly[] assemblies)
        {
            var result = new List<Type>();
            add(self);
            return result;

            void add(Type type)
            {
                if (result.Contains(type)) return;

                if (type.IsConstructedGenericType)
                {
                    foreach (var t in type.GenericTypeArguments) add(t);
                }

                if (type.IsArray)
                {
                    add(type.GetElementType());
                    return;
                }

                if (assemblies.Length > 0 && !assemblies.Contains(type.Assembly)) return;
                if (type.HasCompiledMessagePackFormatter()) return;
                if (type.IsConstructedGenericType &&
                    type.GetGenericTypeDefinition().HasCompiledMessagePackFormatter()) return;

                result.Add(type);
                var children =
                    type.GetProperties()
                        .Where(p => !p.IsIndexed())
                        .Select(p => p.PropertyType)
                        .Distinct()
                        .Where(x => !x.IsEnum);
                var derivations = type.GetSubTypes();

                foreach (var t in children.Concat(derivations)) add(t);
            }
        }

        public static IEnumerable<Type> GetSubTypes(this Type self, params Assembly[] assemblies)
        {
            assemblies = GetAssemblies(self, assemblies);

            return assemblies.SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && t.IsDerivedFrom(self));
        }

        public static bool HasCompiledMessagePackFormatter(this Type self) =>
            _builtinMessagePackTypes.Contains(self);

        public static bool IsDerivedFrom(this Type self, Type baseType) =>
            self != baseType && baseType.IsAssignableFrom(self);

        public static bool IsIndexed(this PropertyInfo self) => self.GetIndexParameters().Any();

        static Assembly[] GetAssemblies(Type type, Assembly[] assemblies) =>
            assemblies.Length == 0 ? new[] { type.Assembly } : assemblies;
    }
}