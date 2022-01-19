using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MessagePack.Attributeless.CompileTime.CodeGeneration
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        static readonly string _graphAttributeName = typeof(SerializeGraphAttribute).FullName;
        readonly Templates _templates = new Templates();

        public void Execute(GeneratorExecutionContext context)
        {
            var attributeSymbol = context.Compilation.GetTypeByMetadataName(_graphAttributeName);
            var infos = context.Compilation.SyntaxTrees.SelectMany(extractSerializerInfos);
            foreach (var (serializer, serializationTypes) in infos)
            {
                var source = SourceText.From(new PartialClassContext
                {
                    Formatters = new[]
                    {
                        "BlaFormatter",
                        "BluFormatter"
                    },
                    Name = serializer.ToDisplayString(),
                    Namespace = serializer.ContainingNamespace.ToDisplayString()
                }.RenderTo(_templates.PartialClass), Encoding.UTF8);
                context.AddSource($"{serializer.Name}.Generated", source);
            }

            (INamedTypeSymbol Serializer, INamedTypeSymbol[] RootTypesToGenerateFor)[] extractSerializerInfos(
                SyntaxTree codeFile)
            {
                var semanticModel = context.Compilation.GetSemanticModel(codeFile);
                // pre-filtering with syntactic model because that's faster than the semantic model
                return codeFile
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(c => c.DescendantNodes().OfType<AttributeSyntax>().Any())
                    .Select(extract)
                    .ToArray();

                (INamedTypeSymbol Serializer, INamedTypeSymbol[] RootTypesToGenerateFor) extract(
                    ClassDeclarationSyntax declaredClass)
                {
                    var classSymbol = semanticModel.GetDeclaredSymbol(declaredClass);
                    var rootTypesToGenerateFor = classSymbol
                        .GetAttributes()
                        .Where(isOurAttribute)
                        .SelectMany(typeArguments)
                        .ToArray();
                    return (classSymbol, rootTypesToGenerateFor);
                }

                bool isOurAttribute(AttributeData attribute) =>
                    attribute?.AttributeClass?.Name == attributeSymbol?.Name;

                IEnumerable<INamedTypeSymbol> typeArguments(AttributeData attribute) =>
                    attribute.ConstructorArguments.First()
                        .Values.OfType<TypedConstant>()
                        .Select(c => c.Value)
                        .Cast<INamedTypeSymbol>();
            }
        }

        public void Initialize(GeneratorInitializationContext context) => _templates.LoadTemplates();
    }
}