using System;
using System.Reflection;

namespace MessagePack.Attributeless.Implementation
{
    public interface IPropertyToKeyMapping : IToKeyMapping<PropertyInfo>
    {
        void Ignore(Func<PropertyInfo, bool> predicate);
    }
}