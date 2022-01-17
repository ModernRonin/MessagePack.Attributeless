using System.Collections.Generic;
using System.Linq;
using ApprovalTests;
using FluentAssertions;
using MessagePack.Attributeless.Implementation;
using NUnit.Framework;

namespace MessagePack.Attributeless.Tests.Implementation;

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

    public class Node
    {
        public ICollection<Node> Children { get; set; }
    }

    class TypeUsingExternalGeneric
    {
        public List<Element> Elements { get; set; }
    }

    class ContainsNullable
    {
        public int? Count { get; set; }
        public Samples.Side? Side { get; set; }
    }

    class HasIndexer
    {
        readonly Dictionary<string, Element> _map = new();

        public Element this[string key]
        {
            get => _map[key];
            set => _map[key] = value;
        }

        public ICollection<Node> Nodes { get; set; }
    }

    [Test]
    public void GetReferencedUserTypes_deals_correctly_with_nullables()
    {
        typeof(ContainsNullable).GetReferencedUserTypes()
            .Should()
            .BeEquivalentTo(new[]
            {
                typeof(ContainsNullable),
                typeof(Samples.Side)
            });
    }

    [Test]
    public void GetReferencedUserTypes_does_not_include_indexed_properties()
    {
        typeof(HasIndexer).GetReferencedUserTypes()
            .Should()
            .BeEquivalentTo(new[]
            {
                typeof(HasIndexer),
                typeof(Node)
            });
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
    public void GetReferencedUserTypes_with_external_generic_doesnt_include_that()
    {
        var type = typeof(TypeUsingExternalGeneric);

        type.GetReferencedUserTypes()
            .Should()
            .BeEquivalentTo(new[]
            {
                type,
                typeof(Element)
            });
    }

    [Test]
    public void GetReferencedUserTypes_with_generic_type_with_type_parameter_in_assembly()
    {
        var type = typeof(UserOfGenericWithTypeParameterInAssembly);

        type.GetReferencedUserTypes()
            .Should()
            .BeEquivalentTo(new[]
            {
                type,
                typeof(Element),
                typeof(Generic<Element>)
            });
    }

    [Test]
    public void GetReferencedUserTypes_with_generic_type_with_type_parameter_outside_assembly()
    {
        var type = typeof(UserOfGenericWithTypeParameterOutsideAssembly);

        type.GetReferencedUserTypes()
            .Should()
            .BeEquivalentTo(new[]
            {
                type,
                typeof(Generic<string>)
            });
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

        type.GetReferencedUserTypes().Should().BeEquivalentTo(new[] { type });
    }

    [Test]
    public void GetReferencedUserTypes_with_other_type_inside_collection()
    {
        var type = typeof(Samples.Person);

        type.GetReferencedUserTypes()
            .Should()
            .BeEquivalentTo(new[]
            {
                type,
                typeof(Samples.Address)
            });
    }

    [Test]
    [Timeout(500)]
    public void GetReferencedUserTypes_with_self_referential_structure()
    {
        typeof(Node).GetReferencedUserTypes().Should().Equal(typeof(Node));
    }
}