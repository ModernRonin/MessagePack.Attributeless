using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessagePack.Attributeless.CompileTime.Templating
{
    /// <summary>
    ///     This poor man's template engine exists only because it's so unbelievably hard to reference nugets in source
    ///     generators.
    /// </summary>
    public class TemplateFiller
    {
        readonly Stack<Dictionary<string, object>> _contextStack = new Stack<Dictionary<string, object>>();
        readonly StringBuilder _result = new StringBuilder();
        int _loopIndex = -1;

        public string Fill(string template, object variables)
        {
            PushContext(variables);

            var lines = template.Split(new[]
            {
                '\n',
                '\r'
            }, StringSplitOptions.RemoveEmptyEntries);
            var lineNumber = 0;
            var startOfLoopLineNumber = -1;
            var lastLoopIndex = -1;
            while (lineNumber < lines.Length && !IsInLoop)
            {
                var line = lines[lineNumber];
                var trimmed = line.Trim();
                if (trimmed.StartsWith("{%"))
                {
                    var collectionLength = ProcessDirective(trimmed);
                    if (IsInLoop)
                    {
                        _loopIndex = 0;
                        lastLoopIndex = collectionLength - 1;
                        startOfLoopLineNumber = lineNumber;
                    }
                    else
                    {
                        if (_loopIndex == lastLoopIndex)
                        {
                            _loopIndex = -1;
                            startOfLoopLineNumber = -1;
                            lastLoopIndex = -1;
                        }
                        else
                        {
                            ++_loopIndex;
                            lineNumber = startOfLoopLineNumber;
                        }
                    }
                }
                else ProcessLine(line);

                ++lineNumber;
            }

            return _result.ToString();
        }

        object GetProperty(string name)
        {
            var isNested = name.Contains(".");
            if (isNested != IsInLoop)
                throw new TemplateException($"Cannot access nested variable '{name}' outside of loop");

            var (source, key) = IsInLoop
                ? ((Dictionary<string, object>)CurrentContext[_loopIndex.ToString()], name.After("."))
                : (CurrentContext, name);

            if (!source.ContainsKey(key))
                throw new TemplateException($"Unknown variable '{name}' in current context: {source}");

            return source[key];
        }

        void PopContext() => _contextStack.Pop();

        int ProcessDirective(string line)
        {
            if (IsInLoop) throw new TemplateException("nested loops are not supported");
            var loopDef = line.After("{%").Before("-%}").Before("%}").Trim();
            var parts = loopDef.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            switch (parts.Length)
            {
                case 4 when parts[0] == "for" && parts[2] == "in":
                {
                    var collectionPropertyName = parts[3];
                    var collection = GetProperty(collectionPropertyName) as IEnumerable;
                    if (collection == null)
                        throw new TemplateException($"'{collectionPropertyName}' is not enumerable");
                    return PushCollectionContext(collection);
                }
                case 1 when parts[0] == "endfor":
                    PopContext();
                    return 0;
                default: throw new TemplateException($"don't know directive '{loopDef}'");
            }
        }

        void ProcessLine(string line)
        {
            const byte text = 0;
            const byte openBrace = 1;
            const byte invariable = 2;
            const byte closeBrace = 3;
            var state = text;
            var currentVariable = string.Empty;
            foreach (var c in line)
            {
                switch (c, state)
                {
                    case ('{', invariable): throw new TemplateException("too many {");
                    case ('{', text):
                        state = openBrace;
                        break;
                    case ('{', openBrace):
                        state = invariable;
                        break;
                    case ('}', invariable):
                        state = closeBrace;
                        break;
                    case ('}', closeBrace):
                        _result.Append(GetProperty(currentVariable)?.ToString() ?? string.Empty);
                        currentVariable = string.Empty;
                        state = text;
                        break;
                    case (_, invariable):
                        currentVariable += c;
                        break;
                    default:
                        state = text;
                        _result.Append(c);
                        break;
                }
            }

            var last = state switch
            {
                closeBrace => '}',
                openBrace => '{',
                invariable => throw new TemplateException($"unfinished variable '{currentVariable}'"),
                _ => (char)0
            };
            if (last != (char)0) _result.Append(last);

            _result.AppendLine();
        }

        int PushCollectionContext(IEnumerable collection)
        {
            var dictionary = new Dictionary<string, object>();
            var i = 0;
            foreach (var element in collection) dictionary[i++.ToString()] = ToDictionary(element);
            _contextStack.Push(dictionary);
            return i;
        }

        void PushContext(object what) => _contextStack.Push(ToDictionary(what));

        static Dictionary<string, object> ToDictionary(object what) =>
            what.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(what));

        Dictionary<string, object> CurrentContext => _contextStack.Peek();
        bool IsInLoop => _loopIndex >= 0;
    }
}