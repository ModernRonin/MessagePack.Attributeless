using System;
using System.Collections.Generic;
using MessagePack.Formatters;

namespace MessagePack.Attributeless
{
    public sealed class SubTypeFormatter<TBase> : IMessagePackFormatter<TBase>, ISubTypeFormatter
    {
        readonly BidirectionalMap<Type, int> _map = new BidirectionalMap<Type, int>();
        int _nextKey;
        public IReadOnlyDictionary<Type, int> Mappings => _map.LeftToRightView();

        public TBase Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil()) return default;

            var key = reader.ReadInt32();
            if (!_map.ContainsRight(key))
            {
                throw new MessagePackSerializationException(
                    $"Encountered unknown type key {key} for {typeof(TBase).Name} - was this serialized with a differrent configuration?");
            }

            var type = _map.LeftFor(key);
            return (TBase) MessagePackSerializer.Deserialize(type, ref reader, options);
        }

        public void Serialize(ref MessagePackWriter writer, TBase value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            var subType = value.GetType();
            if (!_map.ContainsLeft(subType))
            {
                throw new MessagePackSerializationException(
                    $"Missing configuration for subtype {subType.Name} of {typeof(TBase).Name}");
            }

            var key = _map.RightFor(subType);
            writer.Write(key);
            MessagePackSerializer.Serialize(subType, ref writer, value, options);
        }

        public void RegisterSubType<TSub>() where TSub : TBase => RegisterSubType<TSub>(_nextKey++);

        public void RegisterSubType<TSub>(int key) where TSub : TBase =>
            _map.SetLeftToRight(typeof(TSub), key);
    }
}