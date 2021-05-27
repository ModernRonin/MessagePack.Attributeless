using System;
using System.Collections.Generic;
using System.IO;
using AutoBogus;
using Bogus;
using FluentAssertions;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using NUnit.Framework;

namespace MessagePack.Contractless.Subtypes.Tests
{
    public class SubTypeFormatterTests
    {
        public interface IAnimal 
        {
            string Name { get; set; }
            IExtremity[] Extremities { get; set; }
        }

        public abstract class AnAnimal : IAnimal
        {
            public string Name { get; set; }
            public IExtremity[] Extremities { get; set; }
        }
        public class Mammal : AnAnimal
        {
            public TimeSpan Gestation { get; set; }
            
        }
        public class Bird : AnAnimal
        {
            public TimeSpan IncubationPeriod { get; set; }
        }

        public enum Side {Left, Right}
        public interface IExtremity 
        {
            Side Side { get; set; }
        }

        public abstract class AnExtremity : IExtremity
        {
            public Side Side { get; set; }
        }
        public class Arm : AnExtremity
        {
            public byte NumberOfFingers { get; set; }
        }

        public class Leg : AnExtremity
        {
            public byte NumberOfToes { get; set; }
        }

        public class Wing : AnExtremity
        {
            public int Span { get; set; }
        }
        static IEnumerable<IAnimal> AnimalCases
        {
            get
            {
                yield return new Mammal()
                {
                    Name = "Homo sapiens",
                    Gestation = TimeSpan.FromDays(7 * 40),
                    Extremities = new IExtremity[]
                    {
                        new Arm(){Side = Side.Left, NumberOfFingers = 5},
                        new Arm(){Side = Side.Right, NumberOfFingers = 5},
                        new Leg(){Side = Side.Left, NumberOfToes = 5},
                        new Leg(){Side = Side.Right, NumberOfToes = 5},
                    }
                };
                yield return new Bird()
                {
                    Name = "Falco peregrinus",
                    IncubationPeriod = TimeSpan.FromDays(30),
                    Extremities = new IExtremity[]
                    {
                        new Wing(){Side = Side.Left, Span = 120},
                        new Wing(){Side = Side.Right, Span = 120},
                        new Leg(){Side = Side.Left, NumberOfToes = 4},
                        new Leg(){Side = Side.Right, NumberOfToes = 4}
                    }
                };
            }
        }

        static IEnumerable<IExtremity> ExtremityCases
        {
            get
            {
                yield return AutoFaker.Generate<Arm>();
                yield return AutoFaker.Generate<Leg>();
                yield return AutoFaker.Generate<Wing>();
            }
        }
        [TestCaseSource(nameof(ExtremityCases))]
        public void Roundtrip_of_interface(IExtremity input)
        {
            var formatter = new SubTypeFormatter<IExtremity>();
            formatter.RegisterSubType<Arm>(0);
            formatter.RegisterSubType<Leg>(1);
            formatter.RegisterSubType<Wing>(2);
            var options =
                MessagePackSerializer.DefaultOptions.WithResolver(MessagePack.Resolvers.CompositeResolver
                    .Create(new []{formatter}, new []{ContractlessStandardResolver.Instance}));

            using var stream = new MemoryStream();
            MessagePackSerializer.Serialize(stream, input, options);

            stream.Position = 0;
            var output = MessagePackSerializer.Deserialize<IExtremity>(stream, options);

            output.Should().BeEquivalentTo(input, cfg => cfg.RespectingRuntimeTypes());
        }

        [TestCaseSource(nameof(AnimalCases))]
        public void Roundtrip_of_nested_hierarchy(IAnimal input)
        {
            var extremityFormatter = new SubTypeFormatter<IExtremity>();
            extremityFormatter.RegisterSubType<Arm>(0);
            extremityFormatter.RegisterSubType<Leg>(1);
            extremityFormatter.RegisterSubType<Wing>(2);

            var animalFormatter = new SubTypeFormatter<IAnimal>();
            animalFormatter.RegisterSubType<Mammal>(0);
            animalFormatter.RegisterSubType<Bird>(1);

            var options =
                MessagePackSerializer.DefaultOptions.WithResolver(MessagePack.Resolvers.CompositeResolver
                    .Create(new IMessagePackFormatter[]{extremityFormatter, animalFormatter}, new []{ContractlessStandardResolver.Instance}));

            using var stream = new MemoryStream();
            MessagePackSerializer.Serialize(stream, input, options);

            stream.Position = 0;
            var output = MessagePackSerializer.Deserialize<IAnimal>(stream, options);

            output.Should().BeEquivalentTo(input, cfg => cfg.RespectingRuntimeTypes());
            
        }
    }
}