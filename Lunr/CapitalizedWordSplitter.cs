using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class CapitalizedWordSplitter : Pipeline.IFilter
    {
        private static readonly Regex R1 = new Regex(@"(?<=[a-z])[A-Z]+(?=[A-Z])|[A-Z][a-z0-9]+|[a-z0-9]+(?=[A-Z])");
        // private static readonly Regex R2 = new Regex(@"(?<=[a-z])(?=[A-Z])");

        private static readonly Lazy<CapitalizedWordSplitter> Lazy = new Lazy<CapitalizedWordSplitter>(() => new CapitalizedWordSplitter());

        public static CapitalizedWordSplitter Instance => Lazy.Value;

        static CapitalizedWordSplitter()
        {
            Pipeline.RegisterFunction(Instance);
        }

        public object Run(Token token, int i, List<Token> list)
        {
            if (!token.Metadata.ContainsKey("valueParent")) return token;
            var enumerable = Split(token, list).ToList();
            if (!enumerable.Any()) return token;
            return enumerable.ToList();
        }

        private static IEnumerable<Token> Split(Token token, IReadOnlyCollection<Token> list)
        {
            return R1.Matches((string)token.Metadata["valueParent"]).Cast<Match>().Aggregate(new List<Capture>(),
                (match1, match2) =>
                {
                    match1.AddRange(match2.Captures.Cast<Capture>());
                    return match1;
                }).Select((el, j) => Tokenize(el, j, token, list.Count)).Append(token);
        }

        private static Token Tokenize(Capture el, int i, Token originalToken, int ctx)
        {
            var tokenMetadata = originalToken.Metadata;
            var tokenPosition = (int[])tokenMetadata["position"];
            var tokenIndex = (int)tokenMetadata["index"];
            tokenMetadata["position"] = new[] { tokenPosition[0] + el.Index, tokenPosition[1] + el.Value.Length };
            tokenMetadata["index"] = ctx + i;
            tokenMetadata["indexParent"] = tokenIndex;
            tokenMetadata["valueParent"] = el.Value;
            return new Token(el.Value.ToLower(), tokenMetadata);
        }
    }
}