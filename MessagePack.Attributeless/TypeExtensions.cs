using System;

namespace MessagePack.Attributeless
{
    public static class TypeExtensions
    {
        public static bool IsDerivedFrom(this Type self, Type baseType) =>
            self != baseType && baseType.IsAssignableFrom(self);
    }
}