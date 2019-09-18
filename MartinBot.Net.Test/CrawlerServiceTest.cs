using System.Collections.Generic;
using System.Threading.Tasks;
using MartinBot.Net.Config;
using MartinBot.Net.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace MartinBot.Net.Test {
    [TestFixture]
    public class CrawlerServiceTest {

        [SetUp]
        public void Setup () { }

        [Test]
        public void CanGetLineStickerUrlsGreaterThanZero () {
            var logger = new Mock<ILogger<CrawlerService>> ().Object;
            var config = new Mock<IOptions<CrawlerConfig>> ().Object;

            var service = new CrawlerService (config, logger);

            var url = "";
            var result = service.GetLineStickerUrlsAsync (url).Result;
            Assert.Greater (0, result.Count);
        }

    }
}