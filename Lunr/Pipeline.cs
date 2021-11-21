using System;
using System.Collections.Generic;
using System.Linq;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class Pipeline
    {
        private static readonly Dictionary<string, Func<Token, int, List<Token>, object>> RegisteredFunctions = new Dictionary<string, Func<Token, int, List<Token>, object>>();

        private readonly Dictionary<string, Func<Token, int, List<Token>, object>> _stack = new Dictionary<string, Func<Token, int, List<Token>, object>>();

        public static void RegisterFunction(IFilter filter)
        {
            var label = filter.GetType().Name;
            label = label.Substring(0, 1).ToLowerInvariant() + label.Substring(1);

            if (RegisteredFunctions.ContainsKey(label))
            {
                Console.WriteLine("Overwriting existing registered function: " + label);
            }

            RegisteredFunctions[label] = filter.Run;
        }

        public static void WarnIfFunctionNotRegistered(KeyValuePair<string, Func<Token, int, List<Token>, object>> fn)
        {
            var isRegistered = RegisteredFunctions.ContainsKey(fn.Key);
            if (!isRegistered)
            {
                Console.WriteLine("Overwriting extisting registered function: " + fn.Key);
            }
        }

        public void Add(List<Func<Token, int, List<Token>, object>> list)
        {
            foreach (var fn in list)
            {
                _stack.Add(RegisteredFunctions.First((el) => el.Value == fn).Key, fn);
            }
        }

        public void Remove(Func<Token, int, List<Token>, object> fn)
        {
            _stack.Remove(RegisteredFunctions.First((el) => el.Value == fn).Key);
        }

        public List<Token> Run(List<Token> tokens)
        {
            var stackLength = _stack.Count;

            for (var i = 0; i < stackLength; i++)
            {
                var fn = _stack.ElementAt(i).Value;
                var memo = new List<Token>();

                for (var j = 0; j < tokens.Count; j++)
                {
                    var result = fn(tokens[j], j, tokens);

                    if (result == null)
                    {
                        continue;
                    }

                    if (result.GetType() == typeof(List<Token>))
                    {
                        for (var k = 0; k < ((List<Token>)result).Count; k++)
                        {
                            memo.Add(((List<Token>)result)[k]);
                        }
                    }
                    else
                    {
                        memo.Add((Token)result);
                    }
                }

                tokens = memo;
            }

            return tokens;
        }

        public IEnumerable<string> ToJson()
        {
            return _stack.Select(fn =>
            {
                WarnIfFunctionNotRegistered(fn);
                return fn.Key;
            }).ToList();
        }

        public interface IFilter
        {
            public object Run(Token token, int i, List<Token> list);
        }
    }
}