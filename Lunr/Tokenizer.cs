using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class Tokenizer
    {
        public static string TokenSeparator = @"[\s\-]+";

        public static List<Token> Tokenize(string field, Dictionary<string, object> metadata)
        {
            var len = field.Length;
            var tokens = new List<Token>();
            for (int sliceEnd = 0, sliceStart = 0; sliceEnd <= len; sliceEnd++)
            {
                var c = (sliceEnd != len ? str[sliceEnd].ToString() : "");
                var sliceLength = sliceEnd - sliceStart;

                if (!Regex.IsMatch(c, TokenSeparator) && sliceEnd != len) continue;

                if (sliceLength > 0)
                {
                    var tokenMetadata = metadata != null
                        ? new Dictionary<string, object>(metadata)
                        : new Dictionary<string, object>();
                    tokenMetadata["position"] = new [] { sliceStart, sliceLength };
                    tokenMetadata["index"] = tokens.Count;

                    tokens.Add(new Token(field.Substring(sliceStart, sliceLength).ToLower(), tokenMetadata));
                }

                sliceStart = sliceEnd + 1;
            }

            return tokens;
        }
    }
}