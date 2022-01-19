using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack.Attributeless.Implementation;

namespace MessagePack.Attributeless.CompileTime.CodeGeneration
{
    public static class GenerationExtensions
    {
        public static IEnumerable<Type> AllTypes(this SubTypeMappedFormatterCollection self)
        {
            return recurse().Distinct();

            IEnumerable<Type> recurse()
            {
                foreach (var (type, formatter) in self)
                {
                    yield return type;
                    foreach (var (subType, _) in formatter.Mappings) yield return subType;
                }
            }
        }

        public static IEnumerable<Type> AllTypes(this PropertyMappedFormatterCollection self)
        {
            return recurse().Distinct();

            IEnumerable<Type> recurse()
            {
                foreach (var (type, formatter) in self)
                {
                    yield return type;
                    foreach (var (property, _) in formatter.Mappings) yield return property.PropertyType;
                }
            }
        }
    }
}