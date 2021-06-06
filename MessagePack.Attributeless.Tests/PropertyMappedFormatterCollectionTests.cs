using System;
using FluentAssertions;
using NUnit.Framework;

namespace MessagePack.Attributeless.Tests
{
    [TestFixture]
    public class PropertyMappedFormatterCollectionTests
    {
        [Test]
        public void Ignore_throws_if_the_target_type_has_not_yet_been_added()
        {
            var underTest = new PropertyMappedFormatterCollection();

            Action action = () => underTest.Ignore(typeof(Samples.Address), _ => true);

            action.Should()
                .ThrowExactly<InvalidOperationException>()
                .WithMessage(
                    "The type Address is not registered. Add Ignore clauses after registering types.");
        }
    }
}