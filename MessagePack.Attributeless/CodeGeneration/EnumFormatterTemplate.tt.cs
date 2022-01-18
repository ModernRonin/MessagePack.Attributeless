using System;

namespace MessagePack.Attributeless.CodeGeneration
{
    public partial class EnumFormatterTemplate
    {
        string ReaderMethod
        {
            get
            {
                var targetType = Type.GetEnumUnderlyingType();
                if (targetType == typeof(sbyte)) return "ReadInt8((sbyte) value)";
                if (targetType == typeof(short)) return "ReadInt16((short) value)";
                if (targetType == typeof(int)) return "ReadInt32((int) value)";
                if (targetType == typeof(long)) return "ReadInt64((long) value)";
                if (targetType == typeof(byte)) return "ReadUInt8((byte) value)";
                if (targetType == typeof(ushort)) return "ReadUInt16((ushort) value)";
                if (targetType == typeof(uint)) return "ReadUInt32((uint) value)";
                if (targetType == typeof(ulong)) return "ReadUInt64((ulong) value)";
                throw new NotImplementedException();
            }
        }

        string WriterMethod
        {
            get
            {
                var targetType = Type.GetEnumUnderlyingType();
                if (targetType == typeof(sbyte)) return "WriteInt8(value)";
                if (targetType == typeof(short)) return "WriteInt16(value)";
                if (targetType == typeof(int)) return "WriteInt32(value)";
                if (targetType == typeof(long)) return "WriteInt64(value)";
                if (targetType == typeof(byte)) return "WriteUInt8(value)";
                if (targetType == typeof(ushort)) return "WriteUInt16(value)";
                if (targetType == typeof(uint)) return "WriteUInt32(value)";
                if (targetType == typeof(ulong)) return "WriteUInt64(value)";
                throw new NotImplementedException();
            }
        }
    }
}