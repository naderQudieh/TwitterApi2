using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using TwitterApi.Models;
using TwitterApi.Services;

namespace TwitterStream.Services
{
    public class TwitterStreamBackGroudService : IHostedService
    {
        private readonly ITwitterApiService _twitterApiService;
        private readonly ITwitterService _twitterService;
        private readonly ILogger _logger;

        public TwitterStreamBackGroudService(ITwitterApiService twitterApiService, ITwitterService twitterService, ILogger logger)
        {
            _twitterApiService = twitterApiService;
            _twitterApiService.OnTweet += OnTweet;
            _twitterService = twitterService;
            _logger = logger;
        }

        private void OnTweet(object sender, TweetDto tweet)
        {
            _twitterService.ProcessTweet(tweet);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tweet stream starting");
            return _twitterApiService.GetStream(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Tweet stream stopping");
            _twitterApiService.Dispose();
            _logger.LogInformation("Tweet stream stopped");
            return Task.CompletedTask;
        }
    }
}
