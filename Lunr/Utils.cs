using System;
using System.Collections.Generic;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class Utils
    {
        public static double InverseDocumentFrequency(Dictionary<string, dynamic> posting, int documentCount)
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