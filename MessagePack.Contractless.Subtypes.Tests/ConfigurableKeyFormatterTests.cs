using NUnit.Framework;

namespace MessagePack.Contractless.Subtypes.Tests
{
    [TestFixture]
    public class ConfigurableKeyFormatterTests
    {

        public class Person
        {
            public string GivenName { get; set; }
            public string Surname { get; set; }
        }
    }
}