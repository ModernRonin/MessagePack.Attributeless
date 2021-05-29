using System;

namespace MessagePack.Attributeless
{
    public static class ConstraintExtensions
    {
        public static void MustBeDefaultConstructable(this Type self)
        {
            if (self.GetConstructor(Type.EmptyTypes) == default)
                Throw(self, "declare a public default (parameterless) constructor");
        }

        public static void MustBeDerivedFrom(this Type self, Type baseType)
        {
            if (!self.IsDerivedFrom(baseType)) Throw(self, $"be derive from {baseType.Name}");
        }

        static void Throw(Type self, string msg)
        {
            throw new ArgumentException($"Type {self.Name} must {msg}");
        }
    }
}