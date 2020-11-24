using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TwitterStream.Models;


namespace TwitterStream.Services
{
    public interface IEmojiService
    {
        IEnumerable<Emoji> GetEmojisFromText(string text);
    }
    public class EmojiService : IEmojiService
    {
        private readonly ILogger _logger;

        private bool _emojiDataLoaded = false;
        private const string EMOJI_DATA_FILE = "Data/emoji.json";

        private Dictionary<string, Emoji> _emojis;
        private Regex _emojiRegex;

        public EmojiService(ILogger logger)
        {
            _logger = logger;
            _emojiRegex = new Regex("");
            _emojis = new Dictionary<string, Emoji>();
        }

        private void LoadEmojiData()
        {
            try
            {
                _emojiDataLoaded = true;

                if (!File.Exists(EMOJI_DATA_FILE))
                {
                    _logger.LogError("EmojiService: Could not locate the emoji data file {DataFile}", EMOJI_DATA_FILE);
                    return;
                }

                var rawEmojiData = File.ReadAllText(EMOJI_DATA_FILE);
                var emojiData = JsonConvert.DeserializeObject<IEnumerable<Emoji>>(rawEmojiData);

                _emojis = emojiData.ToDictionary(x => x.EmojiCharacter.Replace("*", "\\*"), y => y);

                var regexEmojiData = _emojis.Select(x => x.Key);
                _emojiRegex = new Regex($"({string.Join("|", regexEmojiData)})(\uD83C[\uDFFB-\uDFFF])?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmojiService: LoadEmojiData errored.");
            }
        }

        public IEnumerable<Emoji> GetEmojisFromText(string text)
        {
            try
            {
                if (!_emojiDataLoaded)
                {
                    LoadEmojiData();
                }

                var matches = _emojiRegex.Matches(text);
                var missingEmojis = matches.Where(x => !_emojis.ContainsKey(x.Value));
                if (missingEmojis.Any())
                {
                    _logger.LogWarning("EmojiService: GetEmojisFromText emojis:{Emojis} are missing from the data file", string.Join(",", missingEmojis));
                }
                return matches.Where(x => _emojis.ContainsKey(x.Value)).Select(x => _emojis[x.Value]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmojiService: GetEmojisFromText errored for text:{Text}", text);
                return new List<Emoji>();
            }
        }
    }
}
