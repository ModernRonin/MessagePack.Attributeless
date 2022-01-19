using Fluid;

namespace MessagePack.Attributeless.CompileTime.CodeGeneration
{
    public class PartialClassContext
    {
        public string RenderTo(IFluidTemplate template) => template.Render(this);

        public static implicit operator TemplateContext(PartialClassContext context) =>
            new TemplateContext(context);

        public string[] Formatters { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
    }
}