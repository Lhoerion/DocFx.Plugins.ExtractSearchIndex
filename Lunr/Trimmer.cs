using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.DocAsCode.MarkdownLite;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class Trimmer : Pipeline.IFilter
    {
        private static readonly Regex R1 = new Regex(@"^\W+");
        private static readonly Regex R2 = new Regex(@"\W+$");

        private static string Porter(string w, object obj)
        {
            return w.ReplaceRegex(R1, "").ReplaceRegex(R2, "");
        }

        private static readonly Lazy<Trimmer> Lazy =
            new Lazy<Trimmer>(() => new Trimmer());

        public static Trimmer Instance => Lazy.Value;

        static Trimmer()
        {
            Pipeline.RegisterFunction(Instance);
        }

        public object Run(Token token, int i, List<Token> list)
        {
            return token.Update(Porter);
        }
    }
}