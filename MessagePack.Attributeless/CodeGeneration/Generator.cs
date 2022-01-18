using System;
using System.Collections.Generic;
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

            // arriving here means all types without formatter are enum types
            var templates = new List<AFormatterTemplate>();
            templates.AddRange(typesWithoutFormatter.Select(enumTemplate));
            templates.AddRange(configuration.SubTypeMappedTypes.Select(baseTemplate));
            templates.AddRange(configuration.PropertyMappedTypes.Select(concreteTemplate));
            var builder = new StringBuilder();
            builder.Append(new ResolverTemplate
            {
                Namespace = _namespace,
                Formatters = allTypes.Where(t => !t.HasCompiledMessagePackFormatter())
                    .Select(t => $"{t.Name}Formatter")
                    .ToArray()
            }.TransformText());
            return templates.Aggregate(builder, (b, t) => b.Append(t.TransformText())).ToString();

            bool hasNoFormatter(Type type)
            {
                if (type.HasCompiledMessagePackFormatter()) return false;
                if (configuration.PropertyMappedTypes.Select(kvp => kvp.Key).Contains(type)) return false;
                if (configuration.SubTypeMappedTypes.Select(kvp => kvp.Key).Contains(type)) return false;
                return true;
            }

            EnumFormatterTemplate enumTemplate(Type type) =>
                new EnumFormatterTemplate
                {
                    Namespace = _namespace,
                    Type = type
                };

            BaseTypeTemplate baseTemplate(KeyValuePair<Type, ISubTypeFormatter> kvp)
            {
                var (baseType, formatter) = kvp;
                return new BaseTypeTemplate
                {
                    Namespace = _namespace,
                    Type = baseType,
                    Mappings = formatter.Mappings.ToDictionary(t => t.Key.SafeFullName(),
                        t => t.Value)
                };
            }

            ConcreteTypeTemplate concreteTemplate(KeyValuePair<Type, IPropertyFormatter> kvp)
            {
                var (type, formatter) = kvp;
                return new ConcreteTypeTemplate
                {
                    Namespace = _namespace,
                    Type = type,
                    Mappings = formatter.Mappings.OrderBy(m => m.Value)
                        .Select(m => m.Key)
                        .Select(p => (p.Name, p.PropertyType.SafeFullName()))
                        .ToArray()
                };
            }
        }
    }
}