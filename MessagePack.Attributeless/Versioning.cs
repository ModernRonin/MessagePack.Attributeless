using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MessagePack.Attributeless.Implementation;

namespace MessagePack.Attributeless
{
    public sealed class Versioning
    {
        readonly Configuration _configuration;

        public Versioning(Configuration configuration) => _configuration = configuration;

        /// <summary>
        ///     Use this in the handshake of your protocol, when using MessagePack for encoding network traffic, or in the first
        ///     stage
        ///     of your reading from persistence, when using MessagePack for this, to check whether the incoming format is the same
        ///     as your current configuration.
        ///     <para>
        ///         This being a checksum, there is no guarantee that different configurations will yield different values, but
        ///         it is extermely likely - likely enough for practical purposes.
        ///     </para>
        /// </summary>
        public byte[] Checksum =>
            new SHA512Managed().ComputeHash(
                Encoding.UTF8.GetBytes(string.Join("\n", ConfigurationDescription)));

        /// <summary>
        ///     Once you are happy with your configuration, call this and save the output to a reference file you commit. Then
        ///     create a unit-test
        ///     that calls <see cref="ConfigurationDescription" /> and compares it against the contents of your reference file.
        ///     <para>
        ///         This will help making you aware of changes that might happen through things like, for example, renaming types
        ///         or properties or adding new implementations of interfaces.
        ///     </para>
        ///     <para>
        ///         Another use of this might be for debugging, for example when incoming traffic, also using MessagePack, but
        ///         configured in another language, doesn't deserialize successfully or correctly.
        ///     </para>
        /// </summary>
        public IEnumerable<string> ConfigurationDescription
        {
            get
            {
                yield return "---Subtypes---";
                foreach (var (type, mapping) in _configuration.SubTypeMappedTypes)
                {
                    yield return type.FullName;
                    foreach (var (subtype, key) in mapping.Mappings.OrderBy(kvp => kvp.Key.FullName))
                        yield return $"  - {subtype.FullName} : {key}";
                }

                yield return "---Properties---";
                foreach (var (type, mapping) in _configuration.PropertyMappedTypes)
                {
                    yield return type.FullName;
                    foreach (var (property, key) in mapping.Mappings.OrderBy(kvp => kvp.Key.Name))
                        yield return $"  - {property.Name} : {key}";
                }

                yield return "---Overrides---";
                foreach (var (targetType, formatterType) in _configuration.Overrides)
                    yield return $"{targetType.FullName} : {formatterType.FullName}";

                yield return "---Use Native Formatters---";
                yield return _configuration.DoesUseNativeResolvers.ToString();
            }
        }
    }
}