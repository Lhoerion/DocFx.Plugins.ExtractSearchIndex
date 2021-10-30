using System;
using System.Collections.Generic;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class Token : IComparable
    {
        public string Str;
        public Dictionary<string, object> Metadata;

        public Token(string str, Dictionary<string, object> metadata)
        {
            Str = str ?? "";
            Metadata = metadata;
        }

        public Token Update(Func<string, object, string> fn)
        {
            Str = fn(Str, Metadata);
            return this;
        }

        public override string ToString()
        {
            return Str;
        }

        public int CompareTo(object obj)
        {
            return string.CompareOrdinal(Str, ((Token)obj).Str);
        }

        public class Position
        {
            public int Start;

            public int End;

            public Position(int start, int end)
            {
                Start = start;
                End = end;
            }
        }

        public class EqualityComparer : IEqualityComparer<Token> {
            public bool Equals(Token x, Token y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Str == y.Str;
            }

            public int GetHashCode(Token obj)
            {
                return obj.Str != null ? obj.Str.GetHashCode() : 0;
            }
        }
    }
}