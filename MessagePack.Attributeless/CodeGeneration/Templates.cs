using System;
using Fluid;
using MessagePack.Attributeless.Implementation;
using MessagePack.Attributeless.Properties;

namespace MessagePack.Attributeless.CodeGeneration
{
    public class Templates
    {
        readonly FluidParser _parser = new FluidParser();

        public void LoadTemplates()
        {
            Common = load(Resources.Common_template);
            ConcreteType = load(Resources.ConcreteTypeFormatter_template);
            AbstractType = load(Resources.BaseTypeFormatter_template);
            EnumType = load(Resources.EnumFormatter_template);
            ExtensionsType = load(Resources.Extensions_template);

            IFluidTemplate load(string source)
            {
                if (!_parser.TryParse(source, out var result, out var error))
                    throw new ArgumentException(error);

                return result;
            }
        }

        public IFluidTemplate AbstractType { get; private set; }

        public IFluidTemplate Common { get; private set; }
        public IFluidTemplate ConcreteType { get; private set; }
        public IFluidTemplate EnumType { get; private set; }
        public IFluidTemplate ExtensionsType { get; private set; }
    }

    public class CommonContext
    {
        public string Code { get; set; }
        public string Namespace { get; set; }
    }

    public abstract class ATypeContext
    {
        public string FullTypeName => Type.SafeFullName();
        public string IdentifierTypeName => Type.Name;
        public Type Type { get; set; }
    }

    public class ExtensionsContext
    {
        public string[] Formatters { get; set; }
    }

    public class PropertyContext
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class ConcreteTypeContext : ATypeContext
    {
        public PropertyContext[] Properties { get; set; }
    }

    public class SubTypeContext
    {
        public int Key { get; set; }
        public string Type { get; set; }
    }

    public class AbstractTypeContext : ATypeContext
    {
        public SubTypeContext[] SubTypes { get; set; }
    }

    public class EnumTypeContext : ATypeContext
    {
        public string ReaderMethod
        {
            get
            {
                var targetType = Type.GetEnumUnderlyingType();
                if (targetType == typeof(sbyte)) return "ReadInt8()";
                if (targetType == typeof(short)) return "ReadInt16()";
                if (targetType == typeof(int)) return "ReadInt32()";
                if (targetType == typeof(long)) return "ReadInt64()";
                if (targetType == typeof(byte)) return "ReadUInt8()";
                if (targetType == typeof(ushort)) return "ReadUInt16()";
                if (targetType == typeof(uint)) return "ReadUInt32()";
                if (targetType == typeof(ulong)) return "ReadUInt64()";
                throw new NotImplementedException();
            }
        }

        public string WriterMethod
        {
            get
            {
                var targetType = Type.GetEnumUnderlyingType();
                if (targetType == typeof(sbyte)) return "WriteInt8((sbyte) value)";
                if (targetType == typeof(short)) return "WriteInt16((short) value)";
                if (targetType == typeof(int)) return "WriteInt32((int) value)";
                if (targetType == typeof(long)) return "WriteInt64((long) value)";
                if (targetType == typeof(byte)) return "WriteUInt8((byte) value)";
                if (targetType == typeof(ushort)) return "WriteUInt16((ushort) value)";
                if (targetType == typeof(uint)) return "WriteUInt32((uint) value)";
                if (targetType == typeof(ulong)) return "WriteUInt64((ulong) value)";
                throw new NotImplementedException();
            }
        }
    }
}