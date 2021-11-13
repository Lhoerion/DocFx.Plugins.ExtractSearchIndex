using System.Collections.Generic;
using Newtonsoft.Json;

namespace DocFx.Plugins.ExtractSearchIndex
{
    public class SearchIndexItem : Dictionary<string, string>
    {
        [JsonProperty("href")]
        public string Href {
            get => base["href"];
            set => base["href"] = value;
        }

        [JsonProperty("type")]
        public string Type {
            get => base["type"];
            set => base["type"] = value;
        }

        [JsonProperty("title")]
        public string Title {
            get => base["title"];
            set => base["title"] = value;
        }

        [JsonProperty("keyword")]
        public string Keywords {
            get => base["keyword"];
            set => base["keyword"] = value;
        }

        [JsonProperty("lang")]
        public string Languages {
            get => base["lang"];
            set => base["lang"] = value;
        }
    }
}