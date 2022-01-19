using System;

namespace MessagePack.Attributeless.CompileTime.CodeGeneration
{
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