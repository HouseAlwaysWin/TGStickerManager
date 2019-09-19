using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;

namespace MartinBot.Net.Services.interfaces {
    public interface IImageService {
        Task<Telegram.Bot.Types.File> UploadResizeImagesToTG (TelegramBotClient bot, string path, int userId);
    }
}