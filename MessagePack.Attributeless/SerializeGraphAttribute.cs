using System;

namespace MessagePack.Attributeless
{
    public class SerializeGraphAttribute : Attribute
    {
        public SerializeGraphAttribute(params Type[] types) => Types = types;
        public Type[] Types { get; }
    }
}