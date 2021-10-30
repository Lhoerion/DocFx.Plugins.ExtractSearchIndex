#nullable enable
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class FieldRef : IComparable
    {
        public string DocRef;
        public string FieldName;

        public static char Joiner { get; set; } = '/';

        public FieldRef(string docRef, string fieldName)
        {
            DocRef = docRef;
            FieldName = fieldName;
        }

        public override string ToString()
        {
            return FieldName + Joiner + DocRef;
        }

        public int CompareTo(object obj)
        {
            return string.CompareOrdinal(ToString(), obj.ToString());
        }

        public static FieldRef FromString(string s)
        {
            var n = s.IndexOf(Joiner);
            if (n == -1)
            {
                throw new Exception("Malformed field ref string.");
            }
            return new FieldRef(s.Substring(n + 1), s.Substring(0, n));
        }

        public class EqualityComparer : IEqualityComparer<FieldRef> {
            public bool Equals(FieldRef x, FieldRef y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x.GetType() != y.GetType()) return false;
                return x.FieldName == y.FieldName;
            }

            public int GetHashCode(FieldRef obj)
            {
                return obj.FieldName.GetHashCode();
            }
        }

        public class FieldMetadata
        {
            [JsonProperty("extractor")]
            public readonly Func<Dictionary<string, string>, string>? Extractor;

            [JsonProperty("boost")]
            public readonly int? Boost;

            public FieldMetadata(int? boost = null, Func<Dictionary<string, string>, string>? extractor = null)
            {
                Extractor = extractor;
                Boost = boost;
            }
        }
    }
}