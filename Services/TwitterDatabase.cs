using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TwitterStream.Models;
 

namespace TwitterStream.Services
{
    public interface ITwitterDatabase
    {
        TweetStatistics GetTweetStatistics();
        void AddTweetAnalytics(TweetDetails tweetDetails);
        void UpdateEmojiCount(IEnumerable<Emoji> emojis);
        void UpdateHashTagCount(IEnumerable<string> hashtags);
        void UpdateUrlCount(IEnumerable<Uri> urls);
    }
    public class TwitterDatabase : ITwitterDatabase
    {
        private const string _tweetSummaryKey = "TweetSummary";
        private readonly ConcurrentDictionary<string, TweetAnalytics> _tweetAnalyticsData = new ConcurrentDictionary<string, TweetAnalytics>();
        private readonly ConcurrentDictionary<string, int> _hastTagData = new ConcurrentDictionary<string, int>();
        private readonly ConcurrentDictionary<string, int> _urlData = new ConcurrentDictionary<string, int>();
        private readonly ConcurrentDictionary<string, int> _emjoiData = new ConcurrentDictionary<string, int>();

        public TweetStatistics GetTweetStatistics()
        {
            int numberOfTopItems = 5;// for test - you set it as a config
            if (!_tweetAnalyticsData.ContainsKey(_tweetSummaryKey))
            {
                return new TweetStatistics();
            }

            var tweetAnalytics = _tweetAnalyticsData[_tweetSummaryKey];
            var tweetStatistics = new TweetStatistics()
            {
                TotalTweetsRecieved = tweetAnalytics.TotalTweetsRecieved,

                AverageTweetsPerHour = tweetAnalytics.GetTotalTweetsPerTimeInterval(TimeInterval.Hours),
                AverageTweetsPerMinute = tweetAnalytics.GetTotalTweetsPerTimeInterval(TimeInterval.Minutes),
                AverageTweetsPerSecond = tweetAnalytics.GetTotalTweetsPerTimeInterval(TimeInterval.Seconds),

               
                PercentageOfTweetsWithPhoto = tweetAnalytics.GetPercentageOfTweetsWithContentType(ContentType.Photo),
                PercentageOfTweetsWithUrl = tweetAnalytics.GetPercentageOfTweetsWithContentType(ContentType.Url),
                PercentageOfTweetsWithEmojis = tweetAnalytics.GetPercentageOfTweetsWithContentType(ContentType.Emoji),
              
                TopUrDomains = _urlData.OrderByDescending(x => x.Value).ThenBy(x => x.Key).Take(numberOfTopItems).Select(x => new NoOfUses<string> { Item = x.Key, Uses = x.Value }),
                TopHashTags = _hastTagData.OrderByDescending(x => x.Value).ThenBy(x => x.Key).Take(numberOfTopItems).Select(x => new NoOfUses<string> { Item = x.Key, Uses = x.Value }),
                TopEmojis = _emjoiData.OrderByDescending(x => x.Value).ThenBy(x => x.Key).Take(numberOfTopItems).Select(x => new NoOfUses<string> { Item = x.Key, Uses = x.Value }),
            };

            return tweetStatistics;
        }

        public void AddTweetAnalytics(TweetDetails tweetDetails)
        {
            _tweetAnalyticsData.AddOrUpdate(_tweetSummaryKey,
                new TweetAnalytics(tweetDetails),
                (key, currentValue) => currentValue.UpdateAnalytics(tweetDetails));
        }

        public void UpdateEmojiCount(IEnumerable<Emoji> emojis)
        {
            foreach (var emoji in emojis)
            {
                _emjoiData.AddOrUpdate(emoji.EmojiCharacter, 1, (key, currentValue) => currentValue + 1);
            }
        }

        public void UpdateHashTagCount(IEnumerable<string> hashtags)
        {
            foreach (var hashtag in hashtags)
            {
                _hastTagData.AddOrUpdate(hashtag, 1, (key, currentValue) => currentValue + 1);
            }
        }

        public void UpdateUrlCount(IEnumerable<Uri> uris)
        {
            foreach (var uri in uris)
            {
                _urlData.AddOrUpdate(uri.Host, 1, (key, currentValue) => currentValue + 1);
            }
        }

    }
}
