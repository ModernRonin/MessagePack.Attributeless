using System;
using System.Collections.Generic;
using AutoBogus;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using NUnit.Framework;

namespace MessagePack.Contractless.Subtypes.Tests
{
    public class SubTypeFormatterTests
    {
        [TestCaseSource(nameof(ExtremityCases))]
        public void Roundtrip_of_interface(Samples.IExtremity input)
        {
            var formatter = new SubTypeFormatter<Samples.IExtremity>();
            formatter.RegisterSubType<Samples.Arm>(0);
            formatter.RegisterSubType<Samples.Leg>(1);
            formatter.RegisterSubType<Samples.Wing>(2);
            var options =
                MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                    .Create(new[] {formatter}, new[] {ContractlessStandardResolver.Instance}));

            options.TestRoundtrip(input);
        }

        [TestCaseSource(nameof(AnimalCases))]
        public void Roundtrip_of_nested_hierarchy(Samples.IAnimal input)
        {
            var extremityFormatter = new SubTypeFormatter<Samples.IExtremity>();
            extremityFormatter.RegisterSubType<Samples.Arm>(0);
            extremityFormatter.RegisterSubType<Samples.Leg>(1);
            extremityFormatter.RegisterSubType<Samples.Wing>(2);

            var animalFormatter = new SubTypeFormatter<Samples.IAnimal>();
            animalFormatter.RegisterSubType<Samples.Mammal>(0);
            animalFormatter.RegisterSubType<Samples.Bird>(1);

            var options =
                MessagePackSerializer.DefaultOptions.WithResolver(CompositeResolver
                    .Create(new IMessagePackFormatter[]
                    {
                        extremityFormatter,
                        animalFormatter
                    }, new[] {ContractlessStandardResolver.Instance}));

            options.TestRoundtrip(input);
        }

        static IEnumerable<Samples.IAnimal> AnimalCases
        {
            get
            {
                yield return new Samples.Mammal
                {
                    Name = "Homo sapiens",
                    Gestation = TimeSpan.FromDays(7 * 40),
                    Extremities = new Samples.IExtremity[]
                    {
                        new Samples.Arm
                        {
                            Side = Samples.Side.Left,
                            NumberOfFingers = 5
                        },
                        new Samples.Arm
                        {
                            Side = Samples.Side.Right,
                            NumberOfFingers = 5
                        },
                        new Samples.Leg
                        {
                            Side = Samples.Side.Left,
                            NumberOfToes = 5
                        },
                        new Samples.Leg
                        {
                            Side = Samples.Side.Right,
                            NumberOfToes = 5
                        }
                    }
                };
                yield return new Samples.Bird
                {
                    Name = "Falco peregrinus",
                    IncubationPeriod = TimeSpan.FromDays(30),
                    Extremities = new Samples.IExtremity[]
                    {
                        new Samples.Wing
                        {
                            Side = Samples.Side.Left,
                            Span = 120
                        },
                        new Samples.Wing
                        {
                            Side = Samples.Side.Right,
                            Span = 120
                        },
                        new Samples.Leg
                        {
                            Side = Samples.Side.Left,
                            NumberOfToes = 4
                        },
                        new Samples.Leg
                        {
                            Side = Samples.Side.Right,
                            NumberOfToes = 4
                        }
                    }
                };
            }
        }

        static IEnumerable<Samples.IExtremity> ExtremityCases
        {
            get
            {
                yield return AutoFaker.Generate<Samples.Arm>();
                yield return AutoFaker.Generate<Samples.Leg>();
                yield return AutoFaker.Generate<Samples.Wing>();
            }
        }

        // TODO: test with compression, look at MessagePackSerializer.cs:223
    }
}