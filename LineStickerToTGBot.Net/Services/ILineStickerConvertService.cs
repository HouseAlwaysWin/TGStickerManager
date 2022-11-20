using Telegram.Bot.Types;

namespace LineStickerToTGBotAPI.Services
{
    public interface ILineStickerConvertService
    {
        Task HandleErrorAsync(Exception exception);
        Task HandleRequestAsync(Update update);
    }
}