using System;
using System.Collections.Generic;

namespace MessagePack.Contractless.Subtypes.Tests
{
    public static class Samples
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
            public string GivenName { get; set; }
            public string Surname { get; set; }
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

        public interface IAnimal
        {
            IExtremity[] Extremities { get; set; }
            string Name { get; set; }
        }

        public interface IExtremity
        {
            Side Side { get; set; }
        }
    }
}