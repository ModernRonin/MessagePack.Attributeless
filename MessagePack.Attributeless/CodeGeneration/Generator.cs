using System;
using System.Linq;
using System.Text;
using MessagePack.Attributeless.Implementation;

namespace MessagePack.Attributeless.CodeGeneration
{
    public class Generator
    {
        readonly string _namespace = "Generated";

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
                    Namespace = _namespace,
                    Type = type
                };
                result.Append(template.TransformText());
            }

            foreach (var (baseType, formatter) in configuration.SubTypeMappedTypes)
            {
                var template = new BaseTypeTemplate
                {
                    Namespace = _namespace,
                    Type = baseType,
                    Mappings = formatter.Mappings.ToDictionary(t => t.Key.FullName.Replace('+', '.'),
                        t => t.Value)
                };
                result.Append(template.TransformText());
            }
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
}