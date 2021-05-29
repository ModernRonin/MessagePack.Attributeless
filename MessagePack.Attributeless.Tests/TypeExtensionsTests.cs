using FluentAssertions;
using NUnit.Framework;

namespace MessagePack.Attributeless.Tests
{
    [TestFixture]
    public class TypeExtensionsTests
    {
        [Test]
        public void GetReferencedUserTypes_with_abstract_class()
        {
            var type = typeof(Samples.AnExtremity);
            var expected = new[]
            {
                type,
                typeof(Samples.Arm),
                typeof(Samples.Leg),
                typeof(Samples.Wing)
            };

            type.GetReferencedUserTypes().Should().BeEquivalentTo(expected);
        }

        [Test]
        public void GetReferencedUserTypes_with_complex_graph()
        {
            var type = typeof(Samples.PersonWithPet);
            var expected = new[]
            {
                type,
                typeof(Samples.Person),
                typeof(Samples.IAnimal),
                typeof(Samples.Address),
                typeof(Samples.Mammal),
                typeof(Samples.Bird),
                typeof(Samples.IExtremity),
                typeof(Samples.Arm),
                typeof(Samples.Leg),
                typeof(Samples.Wing)
            };

            type.GetReferencedUserTypes().Should().BeEquivalentTo(expected);
        }

        [Test]
        public void GetReferencedUserTypes_with_interface()
        {
            var type = typeof(Samples.IExtremity);
            var expected = new[]
            {
                type,
                typeof(Samples.Arm),
                typeof(Samples.Leg),
                typeof(Samples.Wing)
            };

            type.GetReferencedUserTypes().Should().BeEquivalentTo(expected);
        }

        [Test]
        public void GetReferencedUserTypes_with_only_non_user_types()
        {
            var type = typeof(Samples.Address);

            type.GetReferencedUserTypes().Should().BeEquivalentTo(type);
        }

        [Test]
        public void GetReferencedUserTypes_with_other_type_inside_collection()
        {
            var type = typeof(Samples.Person);

            type.GetReferencedUserTypes().Should().BeEquivalentTo(type, typeof(Samples.Address));
        }
    }
}