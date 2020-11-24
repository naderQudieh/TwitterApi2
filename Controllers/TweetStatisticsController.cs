using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TwitterStream.Models;
using TwitterStream.Services;

namespace TwitterStream.Controllers
{
    [Route("api/tweet")]
    [ApiController]
    public class TweetStatisticsController : ControllerBase
    {

        private readonly ITwitterService  _twitterService;

        public TweetStatisticsController(ITwitterService TwitterService)
        {
            _twitterService = TwitterService;
        }

        [HttpGet("statistics")]
        public ActionResult GetTweetStatistics()
        { 
            TweetStatistics statistics = _twitterService.GetTweetStatistics();
            return Ok(statistics);
        }
    }
}