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
                .AutoKeyed<Samples.Arm>()
                .AutoKeyed<Samples.Leg>()
                .AutoKeyed<Samples.Wing>()
                .AutoKeyed<Samples.Mammal>()
                .AutoKeyed<Samples.Bird>()
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
    }
}