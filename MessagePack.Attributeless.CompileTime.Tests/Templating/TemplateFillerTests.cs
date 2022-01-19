using MessagePack.Attributeless.CompileTime.CodeGeneration;
using MessagePack.Attributeless.CompileTime.Properties;
using MessagePack.Attributeless.CompileTime.Templating;
using MessagePack.Attributeless.Tests;
using NUnit.Framework;

namespace MessagePack.Attributeless.CompileTime.Tests.Templating;

[TestFixture]
public class TemplateFillerTests
{
    [SetUp]
    public void Setup()
    {
        _underTest = new TemplateFiller();
    }

    TemplateFiller _underTest;

    [Test]
    public void BaseTypeFormatterTemplate()
    {
        var result = _underTest.Fill(Resources.BaseTypeFormatter_template,
            new AbstractTypeContext
            {
                Type = typeof(Samples.IAnimal),
                SubTypes = new SubTypeContext[]
                {
                    new()
                    {
                        Key = 0,
                        Type = "Mammal"
                    },
                    new()
                    {
                        Key = 1,
                        Type = "Bird"
                    }
                }
            });
    }
}