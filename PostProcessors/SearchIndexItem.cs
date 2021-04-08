// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information

namespace DocFx.Plugins.ExtractSearchIndex
{
    using Newtonsoft.Json;

    public class SearchIndexItem
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("keywords")]
        public string Keywords { get; set; }

        [JsonProperty("langs")]
        public string Languages { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as SearchIndexItem);
        }

        public bool Equals(SearchIndexItem other)
        {
            if (other == null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(this.Type, other.Type)
                   && string.Equals(this.Title, other.Title)
                   && string.Equals(this.Href, other.Href)
                   && string.Equals(this.Keywords, other.Keywords)
                   && string.Equals(this.Languages, other.Languages);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Title.GetHashCode() ^ Href.GetHashCode() ^ Keywords.GetHashCode() ^ Languages.GetHashCode();
        }
    }
}