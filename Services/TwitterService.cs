using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using TwitterApi.Models;
using TwitterStream.Models;
 

namespace TwitterStream.Services
{
    public interface ITwitterService
    {
        TweetStatistics GetTweetStatistics();
        void ProcessTweet(TweetDto tweet);
    }
    public class TwitterService : ITwitterService
    {
        private readonly ITwitterDatabase _twitterDatabase;
        private readonly IEmojiService _emojiService;
        private readonly ILogger _logger;
        public TwitterService(ITwitterDatabase twitterDatabase,
            IEmojiService emojiService,
            ILogger logger)
        {
            _twitterDatabase = twitterDatabase;
            _emojiService = emojiService;
            _logger = logger;
        }

        public TweetStatistics GetTweetStatistics()
        {
            try
            {
                return _twitterDatabase.GetTweetStatistics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TwitterService: GetTweetStatistics erroed");
                throw;
            }
        }

        public void ProcessTweet(TweetDto tweet)
        {
            if (tweet == null)
            {
                return;
            }

            var tweetDetails = new TweetDetails();

            var tweetMedia = tweet?.Includes?.Media;
            if (tweetMedia != null)
            {
                tweetDetails.HasPhoto = tweetMedia.Any(x => x.Type == TweetMediaTypes.Photo); 
            }

            var tweetEntities = tweet?.Data?.Entities;
            if (tweetEntities != null)
            {
                if (tweetEntities?.HashTags != null)
                {
                    tweetDetails.HasHashTag = tweetEntities.HashTags.Any();
                    if (tweetDetails.HasHashTag)
                    {
                        _twitterDatabase.UpdateHashTagCount(tweetEntities.HashTags.Select(x => x.Tag));
                    }
                }

                if (tweetEntities?.Urls != null)
                {
                    var urls = tweetEntities.Urls.Where(x => x.ExpandedUri != null);
                    tweetDetails.HasUrl = tweetEntities.Urls.Any();
                    if (tweetDetails.HasUrl)
                    {
                        _twitterDatabase.UpdateUrlCount(urls.Select(x => x.ExpandedUri));
                    }
                }
            }

            var tweetText = tweet?.Data?.Text;
            if (!string.IsNullOrEmpty(tweetText))
            {
                var emojis = _emojiService.GetEmojisFromText(tweetText);
                tweetDetails.HasEmoji = emojis.Any();
                if (tweetDetails.HasEmoji)
                {
                    _twitterDatabase.UpdateEmojiCount(emojis);
                }
            }

            _twitterDatabase.AddTweetAnalytics(tweetDetails);

#if DEBUG
            Console.WriteLine(tweet?.Data?.Text);
#endif
        }
    }
}
