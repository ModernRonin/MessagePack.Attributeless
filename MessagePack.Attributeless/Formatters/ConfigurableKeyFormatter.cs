using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MessagePack.Attributeless.Implementation;
using MessagePack.Formatters;

namespace MessagePack.Attributeless.Formatters
{
    /// <summary>
    ///     You can use this type directly if the automatically generated property key mappings don't work for your use-case.
    /// </summary>
    public sealed class ConfigurableKeyFormatter<T> : IMessagePackFormatter<T>, IPropertyFormatter
        where T : new()
    {
        readonly BidirectionalMap<PropertyInfo, int> _map = new BidirectionalMap<PropertyInfo, int>();

        public IReadOnlyDictionary<PropertyInfo, int> Mappings => _map.LeftToRightView();

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil()) return default;

            var numberOfProperties = reader.ReadInt32();
            var result = new T();
            for (var i = 0; i < numberOfProperties; ++i)
            {
                var key = reader.ReadInt32();
                if (!_map.ContainsRight(key))
                {
                    throw new MessagePackSerializationException(
                        $"Encountered unknown property key {key} for {typeof(T).Name} - was this serialized with a different configuration?");
                }

                var property = _map.LeftFor(key);
                var propertyValue =
                    MessagePackSerializer.Deserialize(property.PropertyType, ref reader, options);
                property.SetValue(result, propertyValue);
            }

            return result;
        }

        public void Ignore(Func<PropertyInfo, bool> predicate) => _map.RemoveLeft(predicate);

        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            writer.Write(_map.Count);
            foreach (var (property, key) in _map)
            {
                var propertyValue = property.GetValue(value);
                writer.Write(key);
                MessagePackSerializer.Serialize(property.PropertyType, ref writer, propertyValue, options);
            }
        }

        public void SetKeyFor<TProperty>(int key, Expression<Func<T, TProperty>> accessor)
        {
            var property = accessor.WriteablePropertyInfo();
            _map.SetLeftToRight(property, key);
        }

        public void UseAutomaticKeys()
        {
            var key = 0;
            foreach (var property in typeof(T).SerializeableProperties().OrderBy(p => p.Name))
                _map.SetRightToLeft(key++, property);
        }
    }
}