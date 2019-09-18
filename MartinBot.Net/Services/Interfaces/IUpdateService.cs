using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MartinBot.Net.Services.interfaces {
    public interface IUpdateService {
        Task EchoAsync (Update update);
        Task<bool> AddBatchStickersToSetAsync (List<Telegram.Bot.Types.File> files, int userId, string name, string pngSticker, int emoji);
    }
}