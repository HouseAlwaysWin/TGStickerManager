using System.Collections.Generic;
using System.Threading.Tasks;

namespace MartinBot.Net.Services.interfaces {
    public interface ICrawlerService {
        Task<List<string>> GetLineStickerUrlsAsync (string url);
    }
}