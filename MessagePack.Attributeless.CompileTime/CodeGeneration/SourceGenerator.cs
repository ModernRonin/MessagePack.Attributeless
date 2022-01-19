using Microsoft.CodeAnalysis;

namespace MessagePack.Attributeless.CompileTime.CodeGeneration
{
    [Generator]
    public class SourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            //context.Compilation.
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}