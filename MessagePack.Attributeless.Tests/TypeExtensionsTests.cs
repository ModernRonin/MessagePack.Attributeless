using System.Linq;
using ApprovalTests;
using FluentAssertions;
using NUnit.Framework;

namespace MessagePack.Attributeless.Tests
{
    [TestFixture]
    public class TypeExtensionsTests
    {
        public class Generic<T> { }

        public class Element { }

        public class UserOfGenericWithTypeParameterInAssembly
        {
            public Generic<Element> Elements { get; set; }
        }

        public class UserOfGenericWithTypeParameterOutsideAssembly
        {
            public Generic<string> Elements { get; set; }
        }

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

            var actual = type.GetReferencedUserTypes();

            var asText = string.Join('\n', actual.OrderBy(t => t.Name).Select(t => t.Name));
            Approvals.Verify(asText);
        }

        [Test]
        public void GetReferencedUserTypes_with_generic_type_with_type_parameter_in_assembly()
        {
            var type = typeof(UserOfGenericWithTypeParameterInAssembly);

            type.GetReferencedUserTypes().Should().BeEquivalentTo(type, typeof(Element));
        }

        [Test]
        public void GetReferencedUserTypes_with_generic_type_with_type_parameter_outside_assembly()
        {
            var type = typeof(UserOfGenericWithTypeParameterOutsideAssembly);

            type.GetReferencedUserTypes().Should().BeEquivalentTo(type);
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
        public void GetReferencedUserTypes_with_nested_interface()
        {
            var type = typeof(Samples.IAnimal);
            var expected = new[]
            {
                type,
                typeof(Samples.IExtremity),
                typeof(Samples.Arm),
                typeof(Samples.Leg),
                typeof(Samples.Wing),
                typeof(Samples.Bird),
                typeof(Samples.Mammal)
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