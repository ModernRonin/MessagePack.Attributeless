using System;

namespace MessagePack.Attributeless.CompileTime.Templating
{
    public class TemplateException : Exception
    {
        public TemplateException(string message) : base(message) { }
    }
}