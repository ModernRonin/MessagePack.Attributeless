using System;
using System.Collections.Generic;
using MessagePack.Formatters;

namespace MessagePack.Contractless.Subtypes
{
    public class SubTypeFormatter<TBase> : IMessagePackFormatter<TBase>
    {
        readonly Dictionary<int, Type> _keysToTypes = new Dictionary<int, Type>();
        readonly Dictionary<Type, int> _typesToKeys = new Dictionary<Type, int>();

        public TBase Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var key = reader.ReadInt32();
            if (!_keysToTypes.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    $"Encountered unknown type key {key} for {typeof(TBase).Name} - looks like this was serialized with a version knowing more types than the current configuration.");
            }

            var type = _keysToTypes[key];
            return (TBase) MessagePackSerializer.Deserialize(type, ref reader, options);
        }

        public void Serialize(ref MessagePackWriter writer, TBase value, MessagePackSerializerOptions options)
        {
            var subType = value.GetType();
            if (!_typesToKeys.ContainsKey(subType))
            {
                throw new InvalidOperationException(
                    $"Missing configuration for subtype {subType.Name} of {typeof(TBase).Name}");
            }

            var key = _typesToKeys[subType];
            writer.Write(key);
            MessagePackSerializer.Serialize(subType, ref writer, value, options);
        }

        public void RegisterSubType<TSub>(int key) where TSub : TBase
        {
            _typesToKeys[typeof(TSub)] = key;
            _keysToTypes[key] = typeof(TSub);
        }
    }
}