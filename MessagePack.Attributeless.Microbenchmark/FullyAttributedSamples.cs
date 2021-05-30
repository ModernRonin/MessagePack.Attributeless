using System;
using System.Collections.Generic;
using System.Linq;
using AutoBogus;
using AutoBogus.Conventions;
using Bogus;

namespace MessagePack.Attributeless.Microbenchmark
{
    public static class FullyAttributedSamples
    {
        [MessagePackObject]
        public class Address
        {
            [Key(0)]
            public string City { get; set; }

            [Key(1)]
            public string Country { get; set; }

            [Key(2)]
            public string StreetAddress { get; set; }

            [Key(3)]
            public string ZipCode { get; set; }
        }

        public abstract class AnAnimal : IAnimal
        {
            [Key(0)]
            public IExtremity[] Extremities { get; set; }

            [Key(1)]
            public string Name { get; set; }
        }

        public abstract class AnExtremity : IExtremity
        {
            [Key(0)]
            public Side Side { get; set; }
        }

        [MessagePackObject]
        public class Arm : AnExtremity
        {
            [Key(1)]
            public byte NumberOfFingers { get; set; }
        }

        [MessagePackObject]
        public class Bird : AnAnimal
        {
            [Key(2)]
            public TimeSpan IncubationPeriod { get; set; }
        }

        [MessagePackObject]
        public class Leg : AnExtremity
        {
            [Key(1)]
            public byte NumberOfToes { get; set; }
        }

        [MessagePackObject]
        public class Mammal : AnAnimal
        {
            [Key(2)]
            public TimeSpan Gestation { get; set; }
        }

        [MessagePackObject]
        public class Person
        {
            [Key(0)]
            public IList<Address> Addresses { get; set; }

            [Key(1)]
            public DateTime Birthday { get; set; }

            [Key(2)]
            public string Email { get; set; }

            [Key(3)]
            public string FirstName { get; set; }

            [Key(4)]
            public string LastName { get; set; }
        }

        [MessagePackObject]
        public class PersonWithPet
        {
            [Key(0)]
            public Person Human { get; set; }

            [Key(1)]
            public IAnimal Pet { get; set; }
        }

        [MessagePackObject]
        public class Wing : AnExtremity
        {
            [Key(1)]
            public int Span { get; set; }
        }

        public enum Side
        {
            Left,
            Right
        }

        [Union(0, typeof(Bird))]
        [Union(1, typeof(Mammal))]
        public interface IAnimal
        {
            IExtremity[] Extremities { get; set; }
            string Name { get; set; }
        }

        [Union(0, typeof(Arm))]
        [Union(1, typeof(Leg))]
        [Union(2, typeof(Wing))]
        public interface IExtremity
        {
            Side Side { get; set; }
        }

        public static PersonWithPet[] Create(int size)
        {
            AutoFaker.Configure(builder => { builder.WithConventions(); });
            var faker = new Faker();
            return faker.Make(size, personWithPet).ToArray();

            IExtremity extremity() =>
                faker.PickRandom(new List<IExtremity>
                {
                    AutoFaker.Generate<Arm>(),
                    AutoFaker.Generate<Leg>(),
                    AutoFaker.Generate<Wing>()
                });

            IExtremity[] extremities() => faker.Make(faker.Random.Number(2, 6), extremity).ToArray();

            IAnimal animal() =>
                faker.PickRandom(new List<IAnimal>
                {
                    addExtremities(AutoFaker.Generate<Bird>()),
                    addExtremities(AutoFaker.Generate<Mammal>())
                });

            T addExtremities<T>(T animal) where T : IAnimal
            {
                animal.Extremities = extremities();
                return animal;
            }

            PersonWithPet personWithPet() =>
                new()
                {
                    Human = AutoFaker.Generate<Person>(),
                    Pet = animal()
                };
        }
    }
}