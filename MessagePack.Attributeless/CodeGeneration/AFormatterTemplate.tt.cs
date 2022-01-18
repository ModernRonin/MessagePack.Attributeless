using System;

namespace MessagePack.Attributeless.CodeGeneration
{
    public abstract partial class AFormatterTemplate
    {
        public string Namespace { get; set; }
        public Type Type { get; set; }
        protected string FullTypeName => Type.FullName.Replace('+', '.');
        protected string IdentifierTypeName => Type.Name;
    }
}