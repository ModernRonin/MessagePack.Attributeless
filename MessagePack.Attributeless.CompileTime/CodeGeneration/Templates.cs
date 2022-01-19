using System;
using Fluid;
using MessagePack.Attributeless.CompileTime.Properties;

namespace MessagePack.Attributeless.CompileTime.CodeGeneration
{
    public class Templates
    {
        readonly FluidParser _parser = new FluidParser();

        public void LoadTemplates()
        {
            Common = load(Resources.Common_template);
            ConcreteType = load(Resources.ConcreteTypeFormatter_template);
            AbstractType = load(Resources.BaseTypeFormatter_template);
            EnumType = load(Resources.EnumFormatter_template);
            ExtensionsType = load(Resources.Extensions_template);
            PartialClass = load(Resources.PartialClass_template);

            IFluidTemplate load(string source)
            {
                if (!_parser.TryParse(source, out var result, out var error))
                    throw new ArgumentException(error);

                return result;
            }
        }

        public IFluidTemplate AbstractType { get; private set; }

        public IFluidTemplate Common { get; private set; }
        public IFluidTemplate ConcreteType { get; private set; }
        public IFluidTemplate EnumType { get; private set; }
        public IFluidTemplate ExtensionsType { get; private set; }
        public IFluidTemplate PartialClass { get; private set; }
    }
}