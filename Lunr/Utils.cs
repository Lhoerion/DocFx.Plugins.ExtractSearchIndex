using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class Utils
    {
        public static string TokenSeparator = @"[\s\-]+";

        /** TODO: Type of obj param **/
        public static List<Token> Tokenizer(string field, Dictionary<string, object> metadata)
        {
            var str = field.ToLower();
            var len = str.Length;
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

                    tokens.Add(new Token(str.Substring(sliceStart, sliceLength), tokenMetadata));
                }

                sliceStart = sliceEnd + 1;
            }

            return tokens;
        }

        public static double Idf(Dictionary<string, dynamic> posting, int documentCount)
        {
            var documentsWithTerm = 0;

            foreach(var fieldName in posting)
            {
                if (fieldName.Key == "_index") continue; // Ignore the term index, its not a field
                documentsWithTerm += posting[fieldName.Key].Keys.Count;
            }

            var x = (documentCount - documentsWithTerm + 0.5) / (documentsWithTerm + 0.5);

            return Math.Log(1 + Math.Abs(x));
        }
    }
}