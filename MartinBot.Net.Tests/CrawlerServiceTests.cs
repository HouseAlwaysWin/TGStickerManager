using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MartinBot.Net.Config;
using MartinBot.Net.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace MartinBot.Net.Tests {
    [TestFixture]
    public class CrawlerServiceTests {
        private IOptions<LineStickerInfo> _config;
        private CrawlerService _service;

        [OneTimeSetUp]
        public void GlobalPrepare () {
            /* Set up configuration  */
            var configuration = new ConfigurationBuilder ()
                .SetBasePath (Directory.GetCurrentDirectory ())
                .AddJsonFile ("appsettings.json", false)
                .Build ();

            _config = Options.Create (configuration.GetSection ("CrawlerConfig")
                .Get<LineStickerInfo> ());
        }

        [SetUp]
        public void PerTestPrepare () {
            var logger = new Mock<ILogger<CrawlerService>> ().Object;
            _service = new CrawlerService (_config, logger);
        }

        [Test]
        public async Task CanGetLineStickerUrlsGreaterThanZero () {
            var url = "https://store.line.me/stickershop/product/7920494/zh-Hant?from=sticker";
            var result = await _service.GetLineStickerUrlsAsync (url);
            Assert.Greater (result.Count, 0);
        }

    }
}