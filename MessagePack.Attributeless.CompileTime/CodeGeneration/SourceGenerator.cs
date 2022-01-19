using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MessagePack.Attributeless.CompileTime.CodeGeneration
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        static readonly string _graphAttributeName = typeof(SerializeGraphAttribute).FullName;

        public void Execute(GeneratorExecutionContext context)
        {
            var attributeSymbol = context.Compilation.GetTypeByMetadataName(_graphAttributeName);
            var codeFiles = context.Compilation.SyntaxTrees;

            foreach (var codeFile in codeFiles)
            {
                var semanticModel = context.Compilation.GetSemanticModel(codeFile);
                var classesWithAttributes = codeFile
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Where(c => c.DescendantNodes().OfType<AttributeSyntax>().Any());

                foreach (var declaredClass in classesWithAttributes)
                {
                    var rootTypesToGenerateFor = semanticModel.GetDeclaredSymbol(declaredClass)
                        .GetAttributes()
                        .Where(isOurAttribute)
                        .SelectMany(typeArguments)
                        .ToArray();
                }

                bool isOurAttribute(AttributeData attribute) =>
                    attribute?.AttributeClass?.Name == attributeSymbol?.Name;

                IEnumerable<INamedTypeSymbol> typeArguments(AttributeData attribute) =>
                    attribute.ConstructorArguments.First()
                        .Values.OfType<TypedConstant>()
                        .Select(c => c.Value)
                        .Cast<INamedTypeSymbol>();
            }
            //context.AddSource("Debug.Generated", SourceText.From(buffer.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}