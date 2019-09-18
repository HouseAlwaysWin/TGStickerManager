using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MartinBot.Net.Services.interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MartinBot.Net.Services {
    public class ImageService : IImageService {

        /// <summary>
        /// GetImageSourceFromPath
        /// </summary>
        /// <param name="path">resource path</param>
        /// <param name="userId">telegram message userId(chatId)</param>
        /// <returns></returns>
        public async Task<List<Telegram.Bot.Types.File>> GetImageByPath (TelegramBotClient bot, string path, int userId) {
            List<Telegram.Bot.Types.File> files = new List<Telegram.Bot.Types.File> ();
            using (Image image = Image.Load (path)) {
                image.Mutate (m => m.Resize (512, 512));
                image.Save (path);
                using (FileStream fs = System.IO.File.OpenRead (path)) {
                    var file = await bot.UploadStickerFileAsync (
                        userId: userId,
                        pngSticker: new Telegram.Bot.Types.InputFiles.InputFileStream (fs)
                    );
                    files.Add (file);
                }
            };
            return files;
        }
    }
}