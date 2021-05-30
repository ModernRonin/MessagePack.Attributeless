using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MessagePack.Attributeless
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetReferencedUserTypes(this Type self, params Assembly[] assemblies)
        {
            assemblies = GetAssemblies(self, assemblies);

            var result = new List<Type>();
            add(self);
            return result;

            void add(Type type)
            {
                if (result.Contains(type)) return;

                if (type.IsConstructedGenericType)
                    foreach (var t in type.GenericTypeArguments)
                        add(t);

                if (type.IsArray)
                {
                    add(type.GetElementType());
                    return;
                }

                if (!assemblies.Contains(type.Assembly)) return;

                result.Add(type);
                var children =
                    type.GetProperties()
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

        public static bool IsDerivedFrom(this Type self, Type baseType) =>
            self != baseType && baseType.IsAssignableFrom(self);

        static Assembly[] GetAssemblies(Type type, Assembly[] assemblies) =>
            assemblies.Length == 0 ? new[] {type.Assembly} : assemblies;
    }
}