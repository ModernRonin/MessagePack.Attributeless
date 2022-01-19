using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace MessagePack.Attributeless.CompileTime.CodeGeneration
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver)) return;

            var buffer = new StringBuilder(@"
using System;
namespace Generated
{
    public static class Debug
    {
        public static string Diagnostics 
        {
        get
        {
            return ");

            var classIds = string.Join(", ", receiver.Classes.Select(c => c.Identifier.ToString()));
            buffer.Append($"\"{classIds}\";");
            buffer.Append(@"
        }
        }
    }
}");

            // inject the created source into the users compilation
            context.AddSource("Debug.Generated", SourceText.From(buffer.ToString(), Encoding.UTF8));

            //var sourceText =
            //    SourceText.From(receiver.Attributes.FirstOrDefault()?.ToString() ?? "nothing found",
            //        Encoding.UTF8);
            //context.AddSource("bla.txt", sourceText);
        }

        public void Initialize(GeneratorInitializationContext context) =>
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public class SyntaxReceiver : ISyntaxReceiver
    {
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (!(syntaxNode is ClassDeclarationSyntax klass)) return;
            if (klass.AttributeLists.Count == 0) return;

            Classes.Add(klass);
            Attributes = klass.AttributeLists.SelectMany(x => x.Attributes)
                .Where(a => a.Name.ToString() == "MessagePack.Attributeless.SerializeGraphAttribute")
                .ToArray();
        }

        public AttributeSyntax[] Attributes { get; private set; }

        public List<ClassDeclarationSyntax> Classes { get; } = new List<ClassDeclarationSyntax>();
    }
}