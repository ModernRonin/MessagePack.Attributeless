using System;

namespace MessagePack.Attributeless.CompileTime.Templating
{
    public static class StringExtensions
    {
        public static string After(this string self, string pattern)
        {
            var index = self.IndexOf(pattern, StringComparison.InvariantCulture);
            if (index == -1) return self;
            return self.Substring(index + pattern.Length);
        }

        public static string Before(this string self, string pattern)
        {
            var index = self.LastIndexOf(pattern, StringComparison.InvariantCulture);
            if (index == -1) return self;
            return self.Substring(0, index);
        }
    }
}