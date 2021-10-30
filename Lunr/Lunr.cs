using System;
using System.Collections.Generic;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class Lunr
    {
        public static Index Main(Action<Builder> fn)
        {
            var builder = new Builder();

            builder.Pipeline.Add(
                new List<Func<Token, int, List<Token>, object>>
                {
                    Trimmer.Instance.Run,
                    StopWordFilter.Instance.Run,
                    Stemmer.Instance.Run
                }
            );

            builder.SearchPipeline.Add(
                new List<Func<Token, int, List<Token>, object>>
                {
                    Stemmer.Instance.Run
                }
            );

            fn.Invoke(builder);

            return builder.Build();
        }
    }
}