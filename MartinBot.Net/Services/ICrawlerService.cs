using System.Collections.Generic;
using System.Threading.Tasks;

namespace MartinBot.Net.Services {
    public interface ICrawlerService {
        Task<List<string>> GetLineStickerUrlsAsync (string url);
    }
}