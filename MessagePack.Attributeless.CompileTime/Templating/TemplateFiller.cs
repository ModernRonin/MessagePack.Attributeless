using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace MessagePack.Attributeless.CompileTime.Templating
{
    /// <summary>
    ///     This poor man's template engine exists only because it's so unbelievably hard to reference nugets in source
    ///     generators.
    /// </summary>
    public class TemplateFiller
    {
        public string Fill(string template, object variables)
        {
            var stack = new Stack<object>();
            stack.Push(variables);
            object context() => stack.Peek();

            object getProperty(string name)
            {
                var ctx = context();
                var property = ctx.GetType().GetProperty(name);
                return property.GetValue(ctx);
            }

            var lines = template.Split(new[]
            {
                '\n',
                '\r'
            }, StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("{%"))
                {
                    var loopDef = trimmed.After("{%").Before("-%}").Before("%}").Trim();
                    var parts = loopDef.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    switch (parts.Length)
                    {
                        case 4 when parts[0] == "for" && parts[2] == "in":
                        {
                            IDictionary<string, object> ctx = new ExpandoObject();
                            ctx[parts[1]] = getProperty(parts[3]);
                            stack.Push(ctx);
                            break;
                        }
                        case 1 when parts[0] == "endfor":
                            stack.Pop();
                            break;
                        default: throw new TemplateException($"don't know directive '{loopDef}'");
                    }
                }
                else
                {
                    var open = 0;
                    var currentVariable = string.Empty;
                    foreach (var c in line)
                    {
                        switch (c, open)
                        {
                            case ('{', 2): throw new TemplateException("too many {");
                            case ('{', _):
                                ++open;
                                break;
                            case ('}', 2):
                                --open;
                                break;
                            case ('}', 1):
                                open = 0;
                                result.Append(getProperty(currentVariable)?.ToString() ?? string.Empty);
                                currentVariable = string.Empty;
                                break;
                            case ('}', _): throw new TemplateException("too many }");
                            case (_, 2):
                                currentVariable += c;
                                break;
                            default:
                                result.Append(c);
                                break;
                        }
                    }

                    result.AppendLine();
                }
            }

            return result.ToString();
        }
    }
}