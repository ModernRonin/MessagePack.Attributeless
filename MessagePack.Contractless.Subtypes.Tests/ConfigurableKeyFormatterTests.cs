using System;
using System.Collections.Generic;
using AutoBogus;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using NUnit.Framework;

namespace MessagePack.Contractless.Subtypes.Tests
{
    [TestFixture]
    public class ConfigurableKeyFormatterTests
    {
        public class Person
        {
            public IList<Address> Addresses { get; set; }
            public DateTime Birthday { get; set; }
            public string Email { get; set; }
            public string GivenName { get; set; }
            public string Surname { get; set; }
        }

        public class Address
        {
            public string City { get; set; }
            public string Country { get; set; }
            public string StreetAddress { get; set; }
            public string ZipCode { get; set; }
        }

        [Test]
        public void Roundtrip_complex_object()
        {
            var addressFormatter = new ConfigurableKeyFormatter<Address>();
            addressFormatter.SetKeyFor(0, a => a.City);
            addressFormatter.SetKeyFor(1, a => a.Country);
            addressFormatter.SetKeyFor(2, a => a.ZipCode);
            addressFormatter.SetKeyFor(3, a => a.StreetAddress);
            var personFormatter = new ConfigurableKeyFormatter<Person>();
            personFormatter.SetKeyFor(0, p => p.Addresses);
            personFormatter.SetKeyFor(1, p => p.Birthday);
            personFormatter.SetKeyFor(2, p => p.Email);
            personFormatter.SetKeyFor(3, p => p.GivenName);
            personFormatter.SetKeyFor(4, p => p.Surname);

            var options =
                MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                    .Create(new IMessagePackFormatter[]
                    {
                        addressFormatter,
                        personFormatter,
                        new NativeDateTimeFormatter()
                    }, new[] {ContractlessStandardResolver.Instance}));

            options.TestRoundtrip(AutoFaker.Generate<Person>());
        }

        [Test]
        public void Roundtrip_complex_object_with_automatic_keys()
        {
            var addressFormatter = new ConfigurableKeyFormatter<Address>();
            var personFormatter = new ConfigurableKeyFormatter<Person>();
            addressFormatter.UseAutomaticKeys();
            personFormatter.UseAutomaticKeys();

            var options =
                MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                    .Create(new IMessagePackFormatter[]
                    {
                        addressFormatter,
                        personFormatter,
                        new NativeDateTimeFormatter()
                    }, new[] {ContractlessStandardResolver.Instance}));

            options.TestRoundtrip(AutoFaker.Generate<Person>());
        }

        [Test]
        public void Roundtrip_simple_object()
        {
            var formatter = new ConfigurableKeyFormatter<Address>();
            formatter.SetKeyFor(0, a => a.City);
            formatter.SetKeyFor(1, a => a.Country);
            formatter.SetKeyFor(2, a => a.ZipCode);
            formatter.SetKeyFor(3, a => a.StreetAddress);

            var options =
                MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                    .Create(new[] {formatter}, new[] {ContractlessStandardResolver.Instance}));

            options.TestRoundtrip(new Address
            {
                ZipCode = "10000",
                Country = "USA",
                City = "NYC",
                StreetAddress = "1st Avenue 13"
            });
        }
    }
}