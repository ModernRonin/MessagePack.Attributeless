using System;
using System.Collections.Generic;
using System.Linq;
using AutoBogus;
using AutoBogus.Conventions;
using Bogus;

namespace MessagePack.Attributeless.Microbenchmark
{
    public static class ContractlessSamples
    {
        public class Address
        {
            public string City { get; set; }
            public string Country { get; set; }
            public string StreetAddress { get; set; }
            public string ZipCode { get; set; }
        }

        public abstract class AnAnimal : IAnimal
        {
            public IExtremity[] Extremities { get; set; }
            public string Name { get; set; }
        }

        public abstract class AnExtremity : IExtremity
        {
            public Side Side { get; set; }
        }

        public class Arm : AnExtremity
        {
            public byte NumberOfFingers { get; set; }
        }

        public class Bird : AnAnimal
        {
            public TimeSpan IncubationPeriod { get; set; }
        }

        public class Leg : AnExtremity
        {
            public byte NumberOfToes { get; set; }
        }

        public class Mammal : AnAnimal
        {
            public TimeSpan Gestation { get; set; }
        }

        public class Person
        {
            public IList<Address> Addresses { get; set; }
            public DateTime Birthday { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public class PersonWithPet
        {
            public Person Human { get; set; }
            public IAnimal Pet { get; set; }
        }

        public class Wing : AnExtremity
        {
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