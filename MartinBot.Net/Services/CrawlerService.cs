using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using MartinBot.Net.Config;
using Microsoft.Extensions.Options;

namespace MartinBot.Net.Services {
    public class CrawlerService : ICrawlerService {

        private readonly CrawlerConfig _crawlerConfig;

        public CrawlerService (IOptions<CrawlerConfig> crawlerConfig) {
            _crawlerConfig = crawlerConfig.Value;
        }

        public async Task<string> GetLineStickerUrlsAsync (string url) {
            try {
                var config = AngleSharp.Configuration.Default.WithDefaultLoader ();
                var context = BrowsingContext.New (config);

                string urlPattern = $"{_crawlerConfig.LineStickerHost}\\*";
                Match checkedUrl = Regex.Match (url, urlPattern);
                if (checkedUrl.Success) {
                    var document = await context.OpenAsync (url);
                    var stickerSelector = "span.mdCMN09Image.FnCustomBase";
                    var images = document.QuerySelectorAll (stickerSelector);
                }

            } catch (Exception ex) {

            }
            return "";
        }

    }
}