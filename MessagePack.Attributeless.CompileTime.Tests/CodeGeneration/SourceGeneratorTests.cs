using System.Text;
using MessagePack.Attributeless.CompileTime.CodeGeneration;
using Microsoft.CodeAnalysis.Text;
using NUnit.Framework;

namespace MessagePack.Attributeless.CompileTime.Tests.CodeGeneration;

using VerifyCS = CSharpSourceGeneratorVerifier<SourceGenerator>;

[TestFixture]
public class SourceGeneratorTests
{
    [Test]
    public async Task Test()
    {
        var code = "initial code";
        var generated = "expected generated code";
        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(SourceGenerator), "GeneratedFileName",
                        SourceText.From(generated, Encoding.UTF8, SourceHashAlgorithm.Sha256))
                }
            }
        }.RunAsync();
    }
}