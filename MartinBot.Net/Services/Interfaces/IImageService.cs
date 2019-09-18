using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;

namespace MartinBot.Net.Services.interfaces {
    public interface IImageService {
        Task<List<Telegram.Bot.Types.File>> GetImageByPath (TelegramBotClient bot, string path, int userId);
    }
}