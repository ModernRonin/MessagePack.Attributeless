using System;
using System.Collections.Generic;
using MessagePack.Formatters;

namespace MessagePack.Attributeless
{
    public class SubTypeFormatter<TBase> : IMessagePackFormatter<TBase>, ISubTypeFormatter
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
                throw new InvalidOperationException(
                    $"Encountered unknown type key {key} for {typeof(TBase).Name} - looks like this was serialized with a version knowing more types than the current configuration.");
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
                throw new InvalidOperationException(
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