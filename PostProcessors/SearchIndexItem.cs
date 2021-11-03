// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information

namespace DocFx.Plugins.ExtractSearchIndex
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

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

        [JsonProperty("keywords")]
        public string Keywords {
            get => base["keywords"];
            set => base["keywords"] = value;
        }

        [JsonProperty("langs")]
        public string Languages {
            get => base["langs"];
            set => base["langs"] = value;
        }
    }
}