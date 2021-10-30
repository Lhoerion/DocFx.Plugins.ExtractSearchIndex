using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class Stemmer : Pipeline.IFilter
    {
        private static readonly Dictionary<string, string> Step2List = new Dictionary<string, string>
        {
            { "ational", "ate" },
            { "tional", "tion" },
            { "enci", "ence" },
            { "anci", "ance" },
            { "izer", "ize" },
            { "bli", "ble" },
            { "alli", "al" },
            { "entli", "ent" },
            { "eli", "e" },
            { "ousli", "ous" },
            { "ization", "ize" },
            { "ation", "ate" },
            { "ator", "ate" },
            { "alism", "al" },
            { "iveness", "ive" },
            { "fulness", "ful" },
            { "ousness", "ous" },
            { "aliti", "al" },
            { "iviti", "ive" },
            { "biliti", "ble" },
            { "logi", "log" }
        };

        private static readonly Dictionary<string, string> Step3List = new Dictionary<string, string>
        {
            { "icate", "ic" },
            { "ative", "" },
            { "alize", "al" },
            { "iciti", "ic" },
            { "ical", "ic" },
            { "ful", "" },
            { "ness", "" }
        };

        private static readonly string C = @"[^aeiou]"; // consonant
        private static readonly string V = @"[aeiouy]"; // vowel
        private static readonly string Cs = C + @"[^aeiouy]*"; // consonant sequence
        private static readonly string Vs = V + @"[aeiou]*"; // vowel sequence

        private static readonly string Mgr0 = @"^(" + Cs + ")?" + Vs + Cs; // [C]VC... is m>0
        private static readonly string Meq1 = @"^(" + Cs + ")?" + Vs + Cs + "(" + Vs + ")?$"; // [C]VC[V] is m=1
        private static readonly string Mgr1 = @"^(" + Cs + ")?" + Vs + Cs + Vs + Cs; // [C]VCVC... is m>1
        private static readonly string Sv = @"^(" + Cs + ")?" + V; // vowel in stem

        private static readonly Regex ReMgr0 = new Regex(Mgr0);
        private static readonly Regex ReMgr1 = new Regex(Mgr1);
        private static readonly Regex ReMeq1 = new Regex(Meq1);
        private static readonly Regex ReSv = new Regex(Sv);

        private static readonly Regex Re1A = new Regex(@"^(.+?)(ss|i)es$");
        private static readonly Regex Re21A = new Regex(@"^(.+?)([^s])s$");
        private static readonly Regex Re1B = new Regex(@"^(.+?)eed$");
        private static readonly Regex Re21B = new Regex(@"^(.+?)(ed|ing)$");
        private static readonly Regex Re1B2 = new Regex(@".$");
        private static readonly Regex Re21B2 = new Regex(@"(at|bl|iz)$");
        private static readonly Regex Re31B2 = new Regex(@"([^aeiouylsz])\1$");
        private static readonly Regex Re41B2 = new Regex(@"^" + Cs + V + "[^aeiouwxy]$");

        private static readonly Regex Re1C = new Regex(@"^(.+?[^aeiou])y$");

        private static readonly Regex Re2 =
            new Regex(
                @"^(.+?)(ational|tional|enci|anci|izer|bli|alli|entli|eli|ousli|ization|ation|ator|alism|iveness|fulness|ousness|aliti|iviti|biliti|logi)$");

        private static readonly Regex Re3 = new Regex(@"^(.+?)(icate|ative|alize|iciti|ical|ful|ness)$");

        private static readonly Regex Re4 =
            new Regex(@"^(.+?)(al|ance|ence|er|ic|able|ible|ant|ement|ment|ent|ou|ism|ate|iti|ous|ive|ize)$");

        private static readonly Regex Re24 = new Regex(@"^(.+?)(s|t)(ion)$");

        private static readonly Regex Re5 = new Regex(@"^(.+?)e$");
        private static readonly Regex Re51 = new Regex(@"ll$");
        private static readonly Regex Re35 = new Regex(@"^" + Cs + V + "[^aeiouwxy]$");

        private static string Porter(string w, object obj)
        {
            string stem,
                suffix,
                firstch;
            Regex re,
                re2,
                re3,
                re4;

            if (w.Length < 3) return w;

            firstch = w.Substring(0, 1);
            if (firstch == "y") w = firstch.ToUpper() + w.Substring(1);

            // Step 1a
            re = Re1A;
            re2 = Re21A;

            if (re.IsMatch(w))
                w = re.Replace(w, "$1$2");
            else if (re2.IsMatch(w)) w = re2.Replace(w, "$1$2");

            // Step 1b
            re = Re1B;
            re2 = Re21B;
            if (re.IsMatch(w))
            {
                var fp = re.Match(w);
                re = ReMgr0;
                if (re.IsMatch(fp.Groups[1].Value))
                {
                    re = Re1B2;
                    w = re.Replace(w, "");
                }
            }
            else if (re2.IsMatch(w))
            {
                var fp = re2.Match(w);
                stem = fp.Groups[1].Value;
                re2 = ReSv;
                if (re2.IsMatch(stem))
                {
                    w = stem;
                    re2 = Re21B2;
                    re3 = Re31B2;
                    re4 = Re41B2;
                    if (re2.IsMatch(w))
                    {
                        w = w + "e";
                    }
                    else if (re3.IsMatch(w))
                    {
                        re = Re1B2;
                        w = re.Replace(w, "");
                    }
                    else if (re4.IsMatch(w))
                    {
                        w = w + "e";
                    }
                }
            }

            // Step 1c - replace suffix y or Y by i if preceded by a non-vowel which is not the first letter of the word (so cry -> cri, by -> by, say -> say)
            re = Re1C;
            if (re.IsMatch(w))
            {
                var fp = re.Match(w);
                stem = fp.Groups[1].Value;
                w = stem + "i";
            }

            // Step 2
            re = Re2;
            if (re.IsMatch(w))
            {
                var fp = re.Match(w);
                stem = fp.Groups[1].Value;
                suffix = fp.Groups[2].Value;
                re = ReMgr0;
                if (re.IsMatch(stem)) w = stem + Step2List[suffix];
            }

            // Step 3
            re = Re3;
            if (re.IsMatch(w))
            {
                var fp = re.Match(w);
                stem = fp.Groups[1].Value;
                suffix = fp.Groups[2].Value;
                re = ReMgr0;
                if (re.IsMatch(stem)) w = stem + Step3List[suffix];
            }

            // Step 4
            re = Re4;
            re2 = Re24;
            if (re.IsMatch(w))
            {
                var fp = re.Match(w);
                stem = fp.Groups[1].Value;
                re = ReMgr1;
                if (re.IsMatch(stem)) w = stem;
            }
            else if (re2.IsMatch(w))
            {
                var fp = re2.Match(w);
                stem = fp.Groups[1].Value + fp.Groups[2].Value;
                re2 = ReMgr1;
                if (re2.IsMatch(stem)) w = stem;
            }

            // Step 5
            re = Re5;
            if (re.IsMatch(w))
            {
                var fp = re.Match(w);
                stem = fp.Groups[1].Value;
                re = ReMgr1;
                re2 = ReMeq1;
                re3 = Re35;
                if (re.IsMatch(stem) || re2.IsMatch(stem) && !re3.IsMatch(stem)) w = stem;
            }

            re = Re51;
            re2 = ReMgr1;
            if (re.IsMatch(w) && re2.IsMatch(w))
            {
                re = Re1B2;
                w = re.Replace(w, "");
            }

            // and turn initial Y back to y

            if (firstch == "y") w = firstch.ToLower() + w.Substring(1);

            return w;
        }

        private static readonly Lazy<Stemmer> Lazy =
            new Lazy<Stemmer>(() => new Stemmer());

        public static Stemmer Instance => Lazy.Value;

        static Stemmer()
        {
            Pipeline.RegisterFunction(Instance);
        }

        public object Run(Token token, int i, List<Token> list)
        {
            return token.Update(Porter);
        }
    }
}