using System;
using System.Linq;
using System.Text;
using MessagePack.Attributeless.Implementation;

namespace MessagePack.Attributeless.CodeGeneration
{
    public class Generator
    {
        public string Generate(Configuration configuration)
        {
            var result = new StringBuilder();
            var allTypes = configuration.PropertyMappedTypes.AllTypes()
                .Concat(configuration.SubTypeMappedTypes.AllTypes())
                .SelectMany(TypeExtensions.WithTypeArguments)
                .Distinct()
                .ToArray();
            var typesWithoutFormatter = allTypes.Where(hasNoFormatter).ToArray();
            var nonEnumTypes = typesWithoutFormatter.Where(t => !t.IsEnum).ToArray();
            if (nonEnumTypes.Any())
            {
                var unhandled = string.Join(", ", nonEnumTypes.Select(t => t.FullName));
                throw new ArgumentException(
                    $"{nameof(configuration)} is incomplete - the following types are neither mapped nor handled by MessagePack natively nor enums: {unhandled}");
            }

            // arriving here means all types without formatter are enum types:
            foreach (var type in typesWithoutFormatter)
            {
                var template = new EnumFormatterTemplate
                {
                    Type = type,
                    Namespace = "Generated"
                };
                result.Append(template.TransformText());
            }
            // generate for enums
            // generate for subtyped
            // generate for propertymapped

            return result.ToString();

            bool hasNoFormatter(Type type)
            {
                if (type.HasCompiledMessagePackFormatter()) return false;
                if (configuration.PropertyMappedTypes.Select(kvp => kvp.Key).Contains(type)) return false;
                if (configuration.SubTypeMappedTypes.Select(kvp => kvp.Key).Contains(type)) return false;
                return true;
            }
        }
    }

    public partial class EnumFormatterTemplate
    {
        public string FullTypeName => Type.FullName.Replace('+', '.');
        public string IdentifierTypeName => Type.Name;
        public string Namespace { get; set; }
        public Type Type { get; set; }

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