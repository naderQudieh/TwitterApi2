using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitterApi.Models;

namespace TwitterApi.Services
{
    public interface ITwitterApiService : IDisposable
    {
        Task GetStream(CancellationToken cancellationToken);
        event EventHandler<TweetDto> OnTweet;
    }
    public class TwitterApiService : ITwitterApiService
    {
        private const int READ_TWEET_TIMEOUT = 60000;
        private const string TWEET_STREAM_ENDPOINT = "2/tweets/sample/stream?tweet.fields=created_at,entities&expansions=attachments.media_keys";

        private readonly ILogger _logger;
        private readonly TwitterSettings _twitterSettings;
        private readonly HttpClient _httpClient;
        private bool _streamTweets = false;

        public event EventHandler<TweetDto> OnTweet = delegate { };

        public TwitterApiService(ILogger logger, TwitterSettings twitterSettings)
        {
            _logger = logger;
            _twitterSettings = twitterSettings;
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(_twitterSettings.BaseUrl),
                Timeout = TimeSpan.FromDays(1)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _twitterSettings.BearerToken);
        }

        public async Task GetStream(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TwitterApiService: GetStream started");

            var numberOfErrors = 0;
            var numberOfErrorsAllowed = 10;
            _streamTweets = true;

            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, TWEET_STREAM_ENDPOINT);
            using var response = await _httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            using var body = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var reader = new StreamReader(body, Encoding.GetEncoding("utf-8"));
            while (_streamTweets && numberOfErrors < numberOfErrorsAllowed)
            {
                try
                {
                    await ReadTweetFromStream(reader);
                }
                catch (Exception ex)
                {
                    numberOfErrors += 1;
                    _logger.LogError(ex, "TwitterApiService: ReadTweetFromStream errored. This is the {numberOfErrors} time.", numberOfErrors);
                }
            }

            _logger.LogInformation("TwitterApiService: GetStream stopped. Number of Errors:{NoOfErrors} Number of Errors Allowed:{NoOfErrorsAllowed} ", numberOfErrors, numberOfErrorsAllowed);
        }

        private async Task ReadTweetFromStream(StreamReader reader)
        {
            var requestTask = reader.ReadLineAsync();
            var resultingTask = await Task.WhenAny(requestTask, Task.Delay(READ_TWEET_TIMEOUT)).ConfigureAwait(false);

            var timedOut = resultingTask != requestTask;
            if (timedOut)
            {
                requestTask.Dispose();
                throw new TimeoutException($"TwitterService: Tweet stream timed out. Timeout in milliseconds {READ_TWEET_TIMEOUT}");
            }

            var jsonResponse = await requestTask.ConfigureAwait(false);
            if (string.IsNullOrEmpty(jsonResponse))
            {
                return;
            }

            var tweet = JsonConvert.DeserializeObject<TweetDto>(jsonResponse);
            OnTweet?.Invoke(this, tweet);
        }

        public void Dispose()
        {
            _streamTweets = false;
            _httpClient?.Dispose();
        }
    }
}
