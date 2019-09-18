using System.Threading.Tasks;

namespace MartinBot.Net.Services {
    public interface ICrawlerService {
        Task<string> GetLineStickerUrlsAsync (string url);
    }
}