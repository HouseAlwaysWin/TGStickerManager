using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace MartinBot.Net.Config {
    public class GlobalConfig {
        public BotConfig BotConfig { get; set; }
        public LineStickerConfig LineStickerConfig { get; set; }
    }
}