using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitterStream.Models
{
    public class TweetMediaTypes
    {
        public static string Photo { get { return "photo"; } }

    }
    public class TweetDetails
    {
        public bool HasPhoto { get; set; }
        public bool HasHashTag { get; set; }
        public bool HasUrl { get; set; }
        public bool HasEmoji { get; set; }
    }
    public class TweetAnalytics
    {
        public DateTime StreamStartTime { get; private set; }
        public int TotalTweetsRecieved { get; set; } = 0;
        public int NoOfTweetsWithUrls { get; set; } = 0;
        public int NoOfTweetsWithPhotos { get; set; } = 0;
        public int NoOfTweetsWithHashTags { get; set; } = 0;
        public int NoOfTweetsWithEmojis { get; set; } = 0;

        public TweetAnalytics(TweetDetails tweetDetails)
        {
            StreamStartTime = DateTime.UtcNow;
            UpdateAnalytics(tweetDetails);
        }

        public TweetAnalytics UpdateAnalytics(TweetDetails tweetDetails)
        {
            TotalTweetsRecieved += 1;

            if (tweetDetails.HasPhoto)
            {
                NoOfTweetsWithPhotos += 1;
            }

            if (tweetDetails.HasHashTag)
            {
                NoOfTweetsWithHashTags += 1;
            }
            if (tweetDetails.HasUrl)
            {
                NoOfTweetsWithUrls += 1;
            }
            if (tweetDetails.HasEmoji)
            {
                NoOfTweetsWithEmojis += 1;
            }
            return this;
        }

        public double GetPercentageOfTweetsWithContentType(ContentType contentType)
        {
            double totalTweetsRecievedDouble = TotalTweetsRecieved;
            if (totalTweetsRecievedDouble == 0)
            {
                totalTweetsRecievedDouble = 1;
            }

            switch (contentType)
            {
                case ContentType.Unknown:
                    return 0;
                case ContentType.Url:
                    return NoOfTweetsWithUrls / totalTweetsRecievedDouble;
                case ContentType.Photo:
                    return NoOfTweetsWithPhotos / totalTweetsRecievedDouble;
                case ContentType.HashTag:
                    return NoOfTweetsWithHashTags / totalTweetsRecievedDouble;
                case ContentType.Emoji:
                    return NoOfTweetsWithEmojis / totalTweetsRecievedDouble;
                default:
                    break;
            }

            return 0;
        }

        public double GetTotalTweetsPerTimeInterval(TimeInterval timeInterval)
        {
            var streamDurationInMilliseconds = (DateTime.UtcNow - StreamStartTime).TotalMilliseconds;
            if (streamDurationInMilliseconds == 0)
            {
                streamDurationInMilliseconds = 1;
            }

            var tweetsPerSecond = (TotalTweetsRecieved / streamDurationInMilliseconds) * 1000;

            switch (timeInterval)
            {
                case TimeInterval.Unknown:
                    return 0;
                case TimeInterval.Seconds:
                    return tweetsPerSecond;
                case TimeInterval.Minutes:
                    return tweetsPerSecond * 60;
                case TimeInterval.Hours:
                    return tweetsPerSecond * 60 * 60;
                default:
                    break;
            }

            return 0;
        }
    }
    public class NoOfUses<T> where T : class
    {
        public T Item { get; set; }
        public int Uses { get; set; }

        public override bool Equals(object obj)
        {
            return obj is NoOfUses<T> uses &&
                   EqualityComparer<T>.Default.Equals(Item, uses.Item) &&
                   Uses == uses.Uses;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Item, Uses);
        }
    }
    public class Emoji
    {
        [JsonProperty("char")]
        public string EmojiCharacter { get; set; }
        public string Name { get; set; }
    }

    public enum TimeInterval
    {
        Unknown = 0,
        Seconds = 1,
        Minutes = 2,
        Hours = 3,
    }
    public enum ContentType
    {
        Unknown = 0,
        Url = 1,
        Photo = 2,
        Video = 3,
        HashTag = 4,
        Emoji = 5,
    }
    public class TweetStatistics
    {
        public int TotalTweetsRecieved { get; set; }

        public double AverageTweetsPerSecond { get; set; }
        public double AverageTweetsPerMinute { get; set; }
        public double AverageTweetsPerHour { get; set; }

        public double PercentageOfTweetsWithPhoto { get; set; }
        public double PercentageOfTweetsWithUrl { get; set; }
        public double PercentageOfTweetsWithEmojis { get; set; }

        public IEnumerable<NoOfUses<string>> TopUrDomains { get; set; }
        public IEnumerable<NoOfUses<string>> TopHashTags { get; set; }
        public IEnumerable<NoOfUses<string>> TopEmojis { get; set; }
    }
}
