using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TwitterApi.Models
{
    public class TweetDto
    {
        public TweetEntity Data { get; set; } = new TweetEntity();
        public TweetIncludes Includes { get; set; } = new TweetIncludes();
    }
    public class TweetEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("author_id")]
        public string AuthorId { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("entities")]
        public TweetEntities Entities { get; set; } = new TweetEntities();

        [JsonProperty("truncated")]
        public bool Truncated { get; set; }
    }

    public class TweetEntities
    {
        [JsonProperty("hashtags")]
        public IEnumerable<TweetHashTag> HashTags { get; set; } = new List<TweetHashTag>();

        [JsonProperty("urls")]
        public IEnumerable<TweetUrl> Urls { get; set; } = new List<TweetUrl>();
    }

    public class TweetHashTag
    {
        [JsonProperty("tag")]
        public string Tag { get; set; }
    }

    public class TweetIncludes
    {
        [JsonProperty("media")]
        public IEnumerable<TwitterMedia> Media { get; set; } = new List<TwitterMedia>();
    }
    public class TweetUrl
    {
        [JsonProperty("url")]
        public string TwitterUrl { get; set; }

        [JsonProperty("expanded_url")]
        public string ExpandedUrl { get; set; }

        [JsonProperty("display_url")]
        public string DisplayUrl { get; set; }

        public Uri ExpandedUri
        {
            get
            {
                if (!string.IsNullOrEmpty(ExpandedUrl)
                    && Uri.TryCreate(ExpandedUrl, UriKind.Absolute, out var url))
                {
                    return url;
                }
                return null;
            }
        }
    }
    public class TwitterMedia
    {
        [JsonProperty("media_key")]
        public string MediaKey { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

}
