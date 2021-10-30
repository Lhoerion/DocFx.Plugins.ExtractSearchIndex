using System.Collections.Generic;
using Newtonsoft.Json;

namespace DocFx.Plugins.ExtractSearchIndex.Lunr
{
    public class Index
    {
        [JsonProperty("version")]
        public string Version = "2.3.9";
        [JsonProperty("fields")]
        public List<string> Fields;
        [JsonIgnore]
        public Dictionary<FieldRef,Vector> FieldVectors;
        [JsonProperty("fieldVectors", ItemConverterType = typeof(FieldVectorsConverter))]
        private List<List<object>> _fieldVectorsMapped = new List<List<object>>();
        [JsonIgnore]
        public Dictionary<Token, Dictionary<string, object>> InvertedIndex;
        [JsonProperty("invertedIndex")]
        private List<List<object>> _invertedIndexMapped = new List<List<object>>();
        [JsonIgnore]
        public Pipeline Pipeline;
        [JsonProperty("pipeline")]
        private List<string> _pipelineMapped = new List<string>();

        public Index(Dictionary<Token, Dictionary<string, object>> invertedIndex, Dictionary<FieldRef, Vector> fieldVectors, List<string> fields, Pipeline pipeline)
        {
            InvertedIndex = invertedIndex;
            FieldVectors = fieldVectors;
            Fields = fields;
            Pipeline = pipeline;
        }

        public object ToJson()
        {
            _invertedIndexMapped.Clear();
            _fieldVectorsMapped.Clear();
            _pipelineMapped.Clear();

            var arr = new List<dynamic>(InvertedIndex.Keys);
            arr.Sort();
            foreach (var term in arr)
            {
                _invertedIndexMapped.Add(new List<object> { term.Str, InvertedIndex[term] });
            }
            arr.Clear();
            arr = new List<dynamic>(FieldVectors.Keys);
            foreach (var fieldRef in arr)
            {
                _fieldVectorsMapped.Add(new List<object> { fieldRef.ToString(), FieldVectors[fieldRef].Elements });
            }
            _pipelineMapped.AddRange(Pipeline.ToJson());
            return MemberwiseClone();
        }
    }
}