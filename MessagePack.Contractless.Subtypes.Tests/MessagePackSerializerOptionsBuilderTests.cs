using ApprovalTests;
using NUnit.Framework;

namespace MessagePack.Contractless.Subtypes.Tests
{
    [TestFixture]
    public class MessagePackSerializerOptionsBuilderTests
    {
        MessagePackSerializerOptionsBuilder Configure() =>
            MessagePackSerializer.DefaultOptions.Configure()
                .AutoKeyed<Samples.Address>()
                .AutoKeyed<Samples.Person>()
                .AddNativeFormatters()
                .SubType<Samples.IExtremity, Samples.Arm>()
                .SubType<Samples.IExtremity, Samples.Leg>()
                .SubType<Samples.IExtremity, Samples.Wing>()
                .SubType<Samples.IAnimal, Samples.Mammal>()
                .SubType<Samples.IAnimal, Samples.Bird>()
                .AutoKeyed<Samples.PersonWithPet>();

        [TestCaseSource(typeof(Samples), nameof(Samples.PeopleWithTheirPets))]
        public void Roundtrip_with(Samples.PersonWithPet input)
        {
            var options = Configure().Build();

            options.TestRoundtrip(input);
        }

        [Test]
        public void KeyTable()
        {
            var builder = Configure();
            var keytable = builder.KeyTable;
            // not Environment.NewLine to prevent issues between the platform where the approved file was saved being different from the one the test is executed on 
            var asText = string.Join('\n', keytable);
            Approvals.Verify(asText);
        }
    }
}