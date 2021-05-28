using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MessagePack.Formatters;

namespace MessagePack.Contractless.Subtypes
{
    public class ConfigurableKeyFormatter<T> : IMessagePackFormatter<T>, IPropertyToKeyMapping
        where T : new()
    {
        readonly BidirectionalMap<PropertyInfo, int> _map = new BidirectionalMap<PropertyInfo, int>();

        public ILookup<PropertyInfo, int> Mappings => _map.ToLookupForLeft();

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var result = new T();
            var numberOfProperties = reader.ReadInt32();
            for (var i = 0; i < numberOfProperties; ++i)
            {
                var key = reader.ReadInt32();
                if (!_map.ContainsRight(key))
                {
                    throw new InvalidOperationException(
                        $"Encountered unknown property key {key} for {typeof(T).Name} - looks like this was serialized with configuration different from the current one");
                }

                var property = _map[key];
                var propertyValue =
                    MessagePackSerializer.Deserialize(property.PropertyType, ref reader, options);
                property.SetValue(result, propertyValue);
            }

            return result;
        }

        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            writer.Write(_map.Count);
            foreach (var (property, key) in _map)
            {
                var propertValue = property.GetValue(value);
                writer.Write(key);
                MessagePackSerializer.Serialize(property.PropertyType, ref writer, propertValue, options);
            }
        }

        public void SetKeyFor<TProperty>(int key, Expression<Func<T, TProperty>> accessor)
        {
            var property = (accessor.Body as MemberExpression)?.Member as PropertyInfo;
            precondition(property != default, "must be a property accessor");
            precondition(property.CanWrite, "must be a writeable property");

            _map[key] = property;

            void precondition(bool condition, string message)
            {
                if (!condition) throw new ArgumentException(message, nameof(accessor));
            }
        }

        public void UseAutomaticKeys()
        {
            var key = 0;
            foreach (var property in typeof(T).GetProperties().Where(p => p.CanWrite).OrderBy(p => p.Name))
                _map[key++] = property;
        }
    }
}