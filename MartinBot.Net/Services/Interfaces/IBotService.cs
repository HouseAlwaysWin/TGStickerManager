using Telegram.Bot;

namespace MartinBot.Net.Services.interfaces {
    public interface IBotService {
        TelegramBotClient Client { get; }
    }
}