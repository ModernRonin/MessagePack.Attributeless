using System;
using System.Collections.Generic;

namespace MessagePack.Attributeless
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetReferencedUserTypes(this Type self)
        {
            return new[] {self};
        }

        public static bool IsDerivedFrom(this Type self, Type baseType) =>
            self != baseType && baseType.IsAssignableFrom(self);
    }
}