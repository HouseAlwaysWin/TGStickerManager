using Telegram.Bot.Types;

namespace TGStickerAPI.Services
{
    public interface ITGStickerService
    {
        Task HandleErrorAsync(Exception exception);
        Task HandleRequestAsync(Update update);
    }
}