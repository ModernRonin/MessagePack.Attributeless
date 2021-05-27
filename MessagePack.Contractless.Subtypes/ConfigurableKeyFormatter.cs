using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MessagePack.Formatters;

namespace MessagePack.Contractless.Subtypes
{
    public class ConfigurableKeyFormatter<T> : IMessagePackFormatter<T> where T: new()
    {
        readonly Dictionary<PropertyInfo, int> _propertiesToKeys = new Dictionary<PropertyInfo, int>();
        readonly Dictionary<int, PropertyInfo> _keyToProperties = new Dictionary<int, PropertyInfo>();
        bool _doesKeyFitIntoByte = true;
        bool _doesNumberOfPropertiesFitIntoByte = true;
        public void SetKeyFor<TProperty>(int key, Expression<Func<T, TProperty>> accessor)
        {
            var property = (accessor.Body as MemberExpression)?.Member as PropertyInfo;
            precondition(property!=default, "must be a property accessor");
            precondition(property.CanWrite, "must be a writeable property");

            _propertiesToKeys[property] = key;
            _keyToProperties[key] = property;
            _doesKeyFitIntoByte = doesFitIntoByte(_keyToProperties.Keys.Max());
            _doesNumberOfPropertiesFitIntoByte = doesFitIntoByte( _keyToProperties.Count);

            bool doesFitIntoByte(int val) => val <= byte.MaxValue;
            void precondition(bool condition, string message)
            {
                if (!condition) throw new ArgumentException(message, nameof(accessor));
            }
        }
        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            writeByteOrInt(writer, _doesNumberOfPropertiesFitIntoByte, _propertiesToKeys.Count);

            foreach (var kvp in _propertiesToKeys)
            {
                var property = kvp.Key;
                var key = kvp.Value;
                var propertValue= property.GetValue(value);
                writeByteOrInt(writer, _doesKeyFitIntoByte, key);
                MessagePackSerializer.Serialize(propertValue, options);
            }

            void writeByteOrInt(MessagePackWriter w, bool condition, int val)
            {
                if (condition) w.Write((byte)val);
                else w.Write(val);
            }
        }

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var result = new T();
            var numberOfProperties = readByteOrInt(reader, _doesNumberOfPropertiesFitIntoByte);
            for (var i = 0; i < numberOfProperties; ++i)
            {
                var key = readByteOrInt(reader, _doesKeyFitIntoByte);
                if (!_keyToProperties.ContainsKey(key))
                    throw new InvalidOperationException(
                        $"Encountered unknown property key {key} for {typeof(T).Name} - looks like this was serialized with configuration different from the current one");
                var property = _keyToProperties[key];
                var propertyValue = MessagePackSerializer.Deserialize(property.PropertyType, ref reader, options);
                property.SetValue(result, propertyValue);
            }

            return result;
            int readByteOrInt(MessagePackReader r, bool condition) => condition ? r.ReadInt32() : r.ReadByte();
        }
    }
}