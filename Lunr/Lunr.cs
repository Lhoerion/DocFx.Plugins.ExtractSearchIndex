using System;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public static class Lunr
    {
        public static Index Main(Action<Builder> fn)
        {
            var builder = new Builder();

            builder.Pipeline.Add(
                Trimmer.Instance.Run,
                StopWordFilter.Instance.Run,
                Stemmer.Instance.Run
            );

            builder.SearchPipeline.Add(
                Stemmer.Instance.Run
            );

            fn.Invoke(builder);

            return builder.Build();
        }
    }
}