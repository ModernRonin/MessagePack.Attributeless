using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fluid;
using MessagePack.Attributeless.Implementation;

namespace MessagePack.Attributeless.CompileTime.CodeGeneration
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
            var templates = new Templates();
            templates.LoadTemplates();
            var buffer = new StringBuilder();

            buffer.Append(templates.ExtensionsType.Render(new TemplateContext(new ExtensionsContext
            {
                Formatters = allTypes.Where(t => !t.HasCompiledMessagePackFormatter())
                    .Select(t => $"{t.Name}Formatter")
                    .ToArray()
            })));

            foreach (var ctx in typesWithoutFormatter.Select(enumTypeContext))
                buffer.Append(templates.EnumType.Render(ctx));

            foreach (var ctx in configuration.SubTypeMappedTypes.Select(abstractTypeContext))
                buffer.Append(templates.AbstractType.Render(ctx));

            foreach (var ctx in configuration.PropertyMappedTypes.Select(concreteTypeContext))
                buffer.Append(templates.ConcreteType.Render(ctx));

            return templates.Common.Render(new TemplateContext(new CommonContext
            {
                Namespace = _namespace,
                Code = buffer.ToString()
            }));

            bool hasNoFormatter(Type type)
            {
                if (type.HasCompiledMessagePackFormatter()) return false;
                if (configuration.PropertyMappedTypes.Select(kvp => kvp.Key).Contains(type)) return false;
                if (configuration.SubTypeMappedTypes.Select(kvp => kvp.Key).Contains(type)) return false;
                return true;
            }

            TemplateContext enumTypeContext(Type type) =>
                new TemplateContext(new EnumTypeContext { Type = type });

            TemplateContext abstractTypeContext(KeyValuePair<Type, ISubTypeFormatter> kvp)
            {
                var (baseType, formatter) = kvp;
                var options = new TemplateOptions();
                options.MemberAccessStrategy.Register<SubTypeContext>();
                return new TemplateContext(new AbstractTypeContext
                {
                    Type = baseType,
                    SubTypes = formatter.Mappings.Select(t => new SubTypeContext
                        {
                            Type = t.Key.SafeFullName(),
                            Key = t.Value
                        })
                        .ToArray()
                }, options);
            }

            TemplateContext concreteTypeContext(KeyValuePair<Type, IPropertyFormatter> kvp)
            {
                var (type, formatter) = kvp;
                var options = new TemplateOptions();
                options.MemberAccessStrategy.Register<PropertyContext>();
                return new TemplateContext(new ConcreteTypeContext
                {
                    Type = type,
                    Properties = formatter.Mappings.OrderBy(m => m.Value)
                        .Select(m => m.Key)
                        .Select(p => new PropertyContext
                        {
                            Name = p.Name,
                            Type = p.PropertyType.SafeFullName()
                        })
                        .ToArray()
                }, options);
            }
        }
    }
}