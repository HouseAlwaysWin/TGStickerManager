using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using MartinBot.Net.Config;
using MartinBot.Net.Services.interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MartinBot.Net.Services {
    public class CrawlerService : ICrawlerService {

        private readonly CrawlerConfig _crawlerConfig;
        private readonly ILogger<CrawlerService> _logger;

        public CrawlerService (IOptions<CrawlerConfig> crawlerConfig, ILogger<CrawlerService> logger) {
            _crawlerConfig = crawlerConfig.Value;
            _logger = logger;
        }

        /// <summary>
        /// Get Line Sticker By Crawling  
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<string>> GetLineStickerUrlsAsync (string url) {
            try {
                var config = AngleSharp.Configuration.Default.WithDefaultLoader ();
                var context = BrowsingContext.New (config);

                string urlPattern = $"{_crawlerConfig.LineStickerHost}*";
                Match checkedUrl = Regex.Match (url, urlPattern);
                if (checkedUrl.Success) {
                    var document = await context.OpenAsync (url);
                    var stickerSelector = "span.mdCMN09Image.FnCustomBase";

                    var linkTags = document.QuerySelectorAll (stickerSelector);
                    var styleContents = linkTags.Select (m => m.GetAttribute ("style"));

                    List<string> imageUrls = new List<string> ();
                    foreach (var content in styleContents) {
                        var imageUrl = content.Replace ("background-image:url(", "").Replace ("compress=true);", "");
                        imageUrls.Add (imageUrl);
                    }

                    return imageUrls;
                }
            } catch (Exception ex) {
                _logger.LogError ($"GetLineStickerUrlsAsync() Error:[{ex}]");
            }
            return new List<string> ();
        }

        /// <summary>
        /// TO DO
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> GetLineStickerTitle (string url) {
            return string.Empty;
        }
    }
}