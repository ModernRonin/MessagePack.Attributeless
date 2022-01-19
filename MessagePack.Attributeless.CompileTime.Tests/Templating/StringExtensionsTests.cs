using FluentAssertions;
using MessagePack.Attributeless.CompileTime.Templating;
using NUnit.Framework;

namespace MessagePack.Attributeless.CompileTime.Tests.Templating;

[TestFixture]
public class StringExtensionsTests
{
    [TestCase("hello world", "hello ", "world")]
    [TestCase("hello world", "wo", "rld")]
    [TestCase("hello world", "wx", "hello world")]
    [TestCase("", "wx", "")]
    [TestCase("hello", "", "hello")]
    public void After(string input, string pattern, string expected) =>
        input.After(pattern).Should().Be(expected);

    [TestCase("hello world", "hello ", "")]
    [TestCase("hello world", "wo", "hello ")]
    [TestCase("hello world", "wx", "hello world")]
    [TestCase("", "wx", "")]
    [TestCase("hello", "", "hello")]
    public void Before(string input, string pattern, string expected) =>
        input.Before(pattern).Should().Be(expected);
}