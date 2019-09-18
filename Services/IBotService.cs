using Telegram.Bot;

namespace MartinBot.Net.Services {
    public interface IBotService {
        TelegramBotClient Client { get; }
    }
}