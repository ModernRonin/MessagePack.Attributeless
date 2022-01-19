using System;
using MessagePack.Attributeless.Implementation;

namespace MessagePack.Attributeless.CompileTime.CodeGeneration
{
    public abstract class ATypeContext
    {
        public string FullTypeName => Type.SafeFullName();
        public string IdentifierTypeName => Type.Name;
        public Type Type { get; set; }
    }
}