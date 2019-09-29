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
        private IOptions<LineStickerConfig> _config;

        [OneTimeSetUp]
        public void GlobalPrepare () {
            /* Set up configuration  */
            var configuration = new ConfigurationBuilder ()
                .SetBasePath (Directory.GetCurrentDirectory ())
                .AddJsonFile ("appsettings.json", false)
                .Build ();

            _config = Options.Create (configuration.GetSection ("CrawlerConfig")
                .Get<LineStickerConfig> ());
        }

    }
}