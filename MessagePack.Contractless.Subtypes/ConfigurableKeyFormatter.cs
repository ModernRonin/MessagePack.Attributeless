using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MessagePack.Formatters;

namespace MessagePack.Contractless.Subtypes
{
    public class ConfigurableKeyFormatter<T> : IMessagePackFormatter<T> where T : new()
    {
        readonly Dictionary<int, PropertyInfo> _keyToProperties = new Dictionary<int, PropertyInfo>();
        readonly Dictionary<PropertyInfo, int> _propertiesToKeys = new Dictionary<PropertyInfo, int>();

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var result = new T();
            var numberOfProperties = reader.ReadInt32();
            for (var i = 0; i < numberOfProperties; ++i)
            {
                var key = reader.ReadInt32();
                if (!_keyToProperties.ContainsKey(key))
                {
                    throw new InvalidOperationException(
                        $"Encountered unknown property key {key} for {typeof(T).Name} - looks like this was serialized with configuration different from the current one");
                }

                var property = _keyToProperties[key];
                var propertyValue =
                    MessagePackSerializer.Deserialize(property.PropertyType, ref reader, options);
                property.SetValue(result, propertyValue);
            }

            return result;
        }

        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            writer.Write(_propertiesToKeys.Count);
            foreach (var kvp in _propertiesToKeys)
            {
                var property = kvp.Key;
                var key = kvp.Value;
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

            SetKeyFor(key, property);

            void precondition(bool condition, string message)
            {
                if (!condition) throw new ArgumentException(message, nameof(accessor));
            }
        }

        public void UseAutomaticKeys()
        {
            var key = 0;
            foreach (var property in typeof(T).GetProperties().Where(p => p.CanWrite).OrderBy(p => p.Name))
                SetKeyFor(key++, property);
        }

        void SetKeyFor(int key, PropertyInfo property)
        {
            _propertiesToKeys[property] = key;
            _keyToProperties[key] = property;
        }
    }
}