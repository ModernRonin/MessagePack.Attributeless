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
            if (self.IsConstructedGenericType)
                return self.GenericTypeArguments.SelectMany(t => t.GetReferencedUserTypes(assemblies));
            if (self.IsArray) return self.GetElementType().GetReferencedUserTypes(assemblies);
            if (!assemblies.Contains(self.Assembly)) return Enumerable.Empty<Type>();
            var children =
                self.GetProperties()
                    .Select(p => p.PropertyType)
                    .Distinct()
                    .Where(x => !x.IsEnum);
            var derivations = self.GetSubTypes();
            return children.Concat(derivations)
                .SelectMany(t => t.GetReferencedUserTypes(assemblies))
                .Distinct()
                .Append(self);
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