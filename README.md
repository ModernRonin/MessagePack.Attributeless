# MessagePack.Attributeless
![CI Status](https://github.com/ModernRonin/MessagePack.Attributeless/actions/workflows/dotnet.yml/badge.svg)
[![NuGet](https://img.shields.io/nuget/v/MessagePack.Attributeless.svg)](https://www.nuget.org/packages/MessagePack.Attributeless/)
[![NuGet](https://img.shields.io/nuget/dt/MessagePack.Attributeless.svg)](https://www.nuget.org/packages/MessagePack.Attributeless)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](http://makeapullrequest.com) 

## Table of Contents
[Motivation](#motivation)
[Release History](#release-history)
[Usage](#usage)
[Performance](#performance)
[Limitations](#limitations)
[License](#license)

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


## Release History
1.0.0: initial release

## Usage

## Performance

## Limitations


## License
The [license](./LICENSE) is [Creative Commons BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/). In essence this means you are free to use and distribute and change this tool however you see fit, as long as you provide a link to the license
and share any customizations/changes you might perform under the same license. 


