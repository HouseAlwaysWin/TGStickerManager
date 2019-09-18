using System.Net.Http;
using MartinBot.Net.Config;
using MartinBot.Net.Services.interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace MartinBot.Net.Services {
    public class BotService : IBotService {
        public readonly BotConfig _botConfig;

        public TelegramBotClient Client { get; }

        public BotService (IOptions<BotConfig> botConfig) {
            _botConfig = botConfig.Value;
            Client = new TelegramBotClient (_botConfig.BotToken);
        }
    }
}