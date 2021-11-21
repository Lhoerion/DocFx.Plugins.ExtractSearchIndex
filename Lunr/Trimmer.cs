using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class Trimmer : Pipeline.IFilter
    {
        private static readonly Regex R1 = new Regex(@"^\W+");
        private static readonly Regex R2 = new Regex(@"\W+$");

        private static string Trim(string w, object obj)
        {
            return R2.Replace(R1.Replace(w, ""), "");
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
            return token.Update(Trim);
        }
    }
}