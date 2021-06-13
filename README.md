# MessagePack.Attributeless
![CI Status](https://github.com/ModernRonin/MessagePack.Attributeless/actions/workflows/dotnet.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/MessagePack.Attributeless.svg)](https://www.nuget.org/packages/MessagePack.Attributeless/)
[![NuGet](https://img.shields.io/nuget/dt/MessagePack.Attributeless.svg)](https://www.nuget.org/packages/MessagePack.Attributeless)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com) 

[Motivation](#motivation) - [Release History](#release-history) - [Usage](#usage) - [Performance](#performance) - [Limitations](#limitations) - [License](#license)

## Motivation
MessagePack is fast and generates small destillates. However, it requires you to either attribute all or many of your types or to give up some of its size gain **and** add a potential security risk.
Out-of-the-box, it offers you basically three choices:

* use the default resolvers: this is where you need to attribute each and every property of your serializables and use additional attributes if you have polymorphic types; for this you get the fastest serialization/deserialization speeds and the smallest destillate size
* use contractless: here you don't need attributes, unless you have polymorphic types; speed is slower and destillate sizes are bigger
* use typeless: here you need to attributes at all, even for polymorphic types; speed is slowest and destillate sizes are biggest; in addition you risk deserializing a malicious destillate that tells the deserializer to create a type you never intended to be deserialized, with all the potential for hackery that follows; this might not affect you, though, if you use MessagePack in a context where you got complete control over destillates so they cannot be tampered with; if, on the other hand, your destillates go over the wire, for example, this is a very real risk

The purpose of MessagePack.Attributeless is to offer a fourth alternative, one that sacrifices some of the blazing speed of MessagePack to get small destillate sizes (almost as small as with the default resolver) with the comfort of not having to attribute your types and the safety of avoiding the kind of type-injection attack Typeless is open to. 

You may wonder what's the problem with attributes. There are quite a few:

1. what if the types are not under your control, that is, you cannot change their source-code?
1. what if you got a few dozen or maybe hundreds of domain types, each with quite a few properties, that you need to serialize? 
1. you bind something that is likely a domain concept to something that is infrastructure
1. if you ever decide to change your serialization format, you'll have to touch all your domain types again

My personal motivation for this project came when I was introducing MessagePack for binary serialization of a huge number of pre-existing domain types at the core of a business-critical application. Those types had been created a long time ago and were already littered with attributes for JSON serialization. JSON serialization was to stay, binary serialization would be used only in certain scenarios to save traffic and lower costs. I really did not want to litter them all with another set of attributes. (Actually, if circumstances allowed, I'd have refactored them to have no JSON attributes either and configured json.net another way.) For one, it would mean touching way too many types, and secondly, the code would have become even more noisy. 

So I looked at Contractless, but that still wouldn't work because of the requirement to attribute polymorphy. Not only would I have to touch a lot types again, even worse, I'd have to let my base-types know about my sub-types. 

Next I looked at Typeless. That was the best option for that project at the time within reasonable time-constraints for our use-case.

But Typeless comes with some disadvantages, as described above. Luckily, the security issue did not affect the described project because clients only ever deserialized binary, only servers serialized it. For our use-case, MessagePack even with Typeless was still vastly superior over JSON, but still it felt like a bit of a shame not to be able to get the full size benefit of MessagePack. 

So I set about trying and finding a better way for these scenarios in my free time. If I'd succeed, I could eventually replace usage at work, if I didn't, I would have wasted my own time and not my client's.

Attributeless is the result of my experiment, and by now it has reached the point where I can use it to replace Typeless even in my client's project.


## Release History
1.0.0: initial release

## Usage
You can pick different levels of using Attributeless, from minimal configuration to relatively low-level.

### GraphOf
The simplest way to use Attributeless looks like this:
```csharp
var options = MessagePackSerializer.DefaultOptions.Configure()
    .GraphOf<Samples.PersonWithPet>()
    .Build();
```
This will walk through the object graph of `PersonWithPet` by looking at any public writeable properties, then at their types and so on, 
while considering polymorphy. For example, `PersonWithPet` is defined as:
```csharp
public class PersonWithPet
{
    public Person Human { get; set; }
    public IAnimal Pet { get; set; }
}
```
Upon encountering the property `Pet`, Attributless will check for all concrete implementations of `IAnimal` and configure itself accordingly. 
If some of your types don't live in the same assembly as your root type, you can pass an array of assemblies to `GraphOf`. If you do this, you need to include
the assembly of the root-type, too. 

What if you want to ignore certain properties? You can ignore them individually like:
```csharp
var options = MessagePackSerializer.DefaultOptions.Configure()
    .GraphOf<Samples.PersonWithPet>()
    .AddNativeFormatters()
    .Ignore<Samples.Address, string>(a => a.City)
    .Ignore<Samples.Address, string>(a => a.Country)
    .Build();
```
or via a predicate like:
```csharp
var options = MessagePackSerializer.DefaultOptions.Configure()
    .GraphOf<Samples.PersonWithPet>()
    .AddNativeFormatters()
    .Ignore<Samples.Address>(pi => pi.Name.StartsWith("C"))
    .Build();
```
What if you need special treatment of one of the types in your type graph?
```csharp
var options = MessagePackSerializer.DefaultOptions.Configure()
    .GraphOf<Samples.PersonWithPet>()
    .AddNativeFormatters()
    .OverrideFormatter<Samples.IExtremity, OverridingExtremityFormatter>()
    .Build();
```
This will override the serialization of `IExtremity` values using your very own formatter which must implement `IMessagePackFormatter<IExtremity>`.

### AutoKeyed, AllSubTypesOf, SubType
What if you don't like the behavior of `.GraphOf`? You can go one step more low-level and use the building blocks upon which `.GraphOf` is based:
* `AutoKeyed` will create configuration for just the type you specify, ignoring any polymorphy; basically, it will create a table of keys for all public writeable properties and use that table for de/serialization - quite the same really as if you manually attributed all properties, sorted by name, with `Key` attributes with incrementing keys. 
* `SubType<TBase, TSub>` will configure `TSub` to be known as a possible sub-type of `TBase`; Attributeless handles polymorphy by serializing an extra integer or byte that specifies the sub-type.
* `AllSubTypesOf<TBase>` does the same, but for all concrete sub-types it can find. If you pass no assemblies, it will look only in the assembly defining `TBase`, otherwise it will look in exactly those assemblies you specify. Why is the assembly defining `TBase` not added implicitly in the later case? Because you might want to exclude implementations of `TBase` in that same assembly.

### Using the formatters themselves
The most low-level usage pattern is to use the two formatters `ConfigurableKeyFormatter<T>` and `SubTypeFormatter<TBase>`. There is also a `NullFormatter<T>` which basically configures MessagePack to completely ignore a certain type. 

The later is not exposed in the fluent builder because there are questions like how to deal with elements in a collection being ignored (do you want them to be `null` values or to be removed or something different altogether) which cannot easily be baked into a generalized API. 

### Protocol versioning
With every serialization protocol based on type and property names, you need to be able to deal with scenarios where types or properties are renamed. More generally, even without the name dependency, you need to deal with destillates serialized by older versions where certain properties or types might have existed or not existed.

MessagePack has some built-in support for supporting fallback scenarios. However, before you can deal with such scenarios, you need to be aware of them. 

Attributeless tries to aid you in this with two methods, `.Versioning.ConfigurationDescription` and `Versioning.Checksum`.

#### Development time

`ConfigurationDescription` is for detecting protocol changes at development time:

Once you are happy with your configuration, you should write the output of `Versioning.ConfigurationDescription` to a file or resource somewhere in your unit-tests. Then you add a test like this:

```csharp
[Test]
public void ConfigurationDescription_is_unchanged()
{
    // assuming this is the way you set up your configuration
    var builder = MessagePackSerializer.DefaultOptions.Configure()
        .GraphOf<Samples.PersonWithPet>()
        .OverrideFormatter<Samples.IExtremity, MySpecialExtremityFormatter>();
    var actual = builder.Versioning.ConfigurationDescription;
    // not Environment.NewLine to prevent issues between the platform where the approved file was saved being different from the one the test is executed on 
    var asText = string.Join('\n', actual);
    Approvals.Verify(asText);
}
```
(If you wonder about the `Approvals.Verify...` line, this uses [ApprovalTests](https://github.com/approvals/ApprovalTests.Net) which I highly recommend for scenarios where you need to verify textual output of more than 1-2 lines. But you could do the same also via something like `asTest.Should().Equal(myLoadedReferenceFileAsString)`.).

Now if someone (might even be you yourself ;P) goes and renames a property or adds a new implementation or whatever, this test will fail. And by comparing the `ConfigurationDescription` you have stored as a reference with the one you are getting in your failed test, you will see what has changed and how to devise a fallback strategy.

In certain cases, no fallback might be necessary, for example if you added types and old clients will never see destillates generated by new clients. 

#### Runtime
To detect protocol mismatches at runtime, you should embed the output of `Versioning.Checksum` somehow in your data flow. 

If you use serialzation in a networking context, sending the checksum would be part of your handshake before you exchange any other messages. 

If you use serialization for persistence, then the checksum should be written into your storage before any of the actual serialized data. Either way, by comparing the checksum you get - be that from the network or from storage - with what you is the output of your *current* configuration, you will be able to detect a protocol/version mismatch.


#### How does it work
ConfigurationDescription is simply a table of all configuration generated via Attributeless. An example for the samples you find in the unit tests [see here for the full type definitions](./MessagePack.Attributeless.Tests/Samples.cs)) with the configuration shown above looks like this:
```text
---Subtypes---
MessagePack.Attributeless.Tests.Samples+IAnimal
  - MessagePack.Attributeless.Tests.Samples+Bird : 0
  - MessagePack.Attributeless.Tests.Samples+Mammal : 1
MessagePack.Attributeless.Tests.Samples+IExtremity
  - MessagePack.Attributeless.Tests.Samples+Arm : 0
  - MessagePack.Attributeless.Tests.Samples+Leg : 1
  - MessagePack.Attributeless.Tests.Samples+Wing : 2
---Properties---
MessagePack.Attributeless.Tests.Samples+Address
  - City : 0
  - Country : 1
  - StreetAddress : 2
  - ZipCode : 3
MessagePack.Attributeless.Tests.Samples+Arm
  - NumberOfFingers : 0
  - Side : 1
MessagePack.Attributeless.Tests.Samples+Bird
  - Extremities : 0
  - IncubationPeriod : 1
  - Name : 2
MessagePack.Attributeless.Tests.Samples+Leg
  - NumberOfToes : 0
  - Side : 1
MessagePack.Attributeless.Tests.Samples+Mammal
  - Extremities : 0
  - Gestation : 1
  - Name : 2
MessagePack.Attributeless.Tests.Samples+Person
  - Addresses : 0
  - Birthday : 1
  - Email : 2
  - FirstName : 3
  - LastName : 4
MessagePack.Attributeless.Tests.Samples+PersonWithPet
  - Human : 0
  - Pet : 1
MessagePack.Attributeless.Tests.Samples+Wing
  - Side : 0
  - Span : 1
---Overrides---
MessagePack.Attributeless.Tests.Samples+IExtremity : MessagePack.Attributeless.Tests.MessagePackSerializerOptionsBuilderTests+MySpecialExtremityFormatter
---Use Native Formatters---
False
```

`Checksum` simply generates a `SHA512` hash of this description. (Note that as with all hashes, there is no absolute guarantee that different versions won't generate the same hash. But given the size of the hash - 512 bits - and the size of `ConfigurationDescription`, it is unlikely enough to rely on it in most scenarios. If you need absolute guarantees, you would have to compare `ConfigurationDescription` directly, but for most cases this will be overkill.)

### Native resolvers
For convenice reasons, there is also a method `.AddNativeFormatters()` which adds all native C# formatters. Of course you should not use this unless all your clients use a CLR language, like C# or F#. But if you're using Attributeless, chances are you don't care about languages outside the .NET eco-system because there is no port of it to other languages (at least so far).

### Non-Generic overloads
All the fluent builder methods also have non-generic overloads (like for example `GraphOf(Type type, params Assembly[] assemblies)` as an alternative to `GraphOf<T>(params Assembly[] assemblies)`) to support scenarios where you want to supply the types dynamically, for example test-cases. 

## Performance
Performance data was generated running the `MessagePack.Attributeless.Microbenchmark` project in release mode outside the debugger with `-n 1000 -r 100 -j`. Feel free to check the source - if you find something you think is not right, feel free to open a discussion.

In all diagrams, JSON is added as a reference.

The following diagram puts the four methods on a coordinate-system between size and speed.

The speed metric is derived from multiplying the average time to serialize one array of 1000 complex elements with the average time to deserialize it, in other words it's directly proportional to serialization and deserialization speed. The axis is logarithmic.

The size metric is simply the size of the destillate, the serialized form of said array of 1000 complex elements, in bytes.

<img src="readmeresources/SpeedAndSize.png" width=600>

As you can see, JSON is by far the slowest while Fully Attributed - the standard resolver of MessagePack - is the fastest.

In absolute terms, the comparison looks like this:

| Method | Serialize (ms) | Deserialize (ms) |
| ------ | -------------- |----------------- |
| Attributeless | 9.2 | 12.0 |
| Fully attributed | 2.2 | 2.0 |
| Contractless | 3.0 | 2.7 |
| Typeless | 5.4 | 5.1 |
| JSON | 34.9 | 62.6 |

If speed is your primary metric, you should use Fully Attributed - it's about 16(!) times faster than JSON for serialization and almost 31 times faster for deserialization. 

Attributeless is clearly the slowest of the four MessagePack methods, but it's still almost 4 times as fast as JSON for serialization and 5 times as fast for deserialization.

Looking at size, on the other hand, surprisingly Typeless is even worse than JSON, and Attributeless is almost as small as the standard resolver.

This is runtime performance - but how about development productivity? How many attributes do we have to add with each method and how many domain types do we need to touch to enable serialization?

| Method | Types touched | Number of Attributes added |
| ------ | ------------- | -------------------------- |
| Fully attributed | 10 | 25 |
| Contractless | 2 | 5 |
| JSON | 0 | 0 |
| Typeless | 0 | 0 |
| Attributeless | 0 | 0 |

As you can see, Attributeless requires no touching of your types at all, same as JSON and Typeless, while Fully Attributed is the most intrusive option.

(In case you wonder how the benchmark dealt with polymorphic types in JSON, take a look at the excellent [JsonSubtypes](https://github.com/manuc66/JsonSubTypes) library.)

Obviously, these numbers very much depend on your domain types. The benchmark, for example, has 2 polymorphic hierarchies. If your domain model has none, then Contractless will require zero types being touched and zero attributes, just like JSON or Typeless or Attributeless. Vice versa, if you got a lot more, the numbers will rise.



### Conclusion - which serializer and settings to pick?
It all depends on your use-case. Here are a few ideas:

Do you have a large domain model or just a few types? If the former, you should seriously consider Attributeless, if the later, Fully Attributed or Contractless might be right for you.

What is more important for you, size or speed? If the former, you should consider Fully Attributed or Attributeless, if the later, Fully Attributed or Contractless.

How dramatic are your speed requirements? If every millisecond counts, you definitely need Fully Attributed. On the other hand, if you need fast, but you don't need to shave of every millisecond possible, Attributeless with its significantly greater development comfort and OOD comformance is likely the right choice.

How likely are you to change your serializing protocol in the future or having to support multiple protocols? If you can commit to MessagePack and MessagePack only for the indefinite future, then Fully Attributed will work for you. If you need to support multiple formats or consider a change of format not unlikely over the lifetime of your product, then you will prefer Attributeless.

## Limitations 
Compared to MessagePack out-of-the-box, Attributeless introduces a few limitations. These limitations are by design and reflect a tradeoff of both lower development and runtime complexity for not supporting certain edge cases:

* Serializable types all must be default-constructable and public 
* Serializable properties must be public and writeable
* indexer properties are not serialized or deserialized

## License
The [license](./LICENSE) is [Creative Commons BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/). In essence this means you are free to use and distribute and change this tool however you see fit, as long as you provide a link to the license
and share any customizations/changes you might perform under the same license. 


