using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MartinBot.Net.Services {
    public interface IUpdateService {
        Task EchoAsync (Update update);

    }
}