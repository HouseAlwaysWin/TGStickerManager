using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MartinBot.Net.Config;
using MartinBot.Net.Services.interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MartinBot.Net.Services {
    public class UpdateService : IUpdateService {

        private readonly IBotService _botService;
        private readonly ICrawlerService _crawlerService;
        private readonly IImageService _imageService;
        private readonly ILogger<UpdateService> _logger;
        private readonly IOptions<CrawlerConfig> _crawlerConfig;

        public UpdateService (
            IBotService botService,
            ICrawlerService crawlerService,
            IOptions<CrawlerConfig> crawlerConfig,
            IImageService imageService,
            ILogger<UpdateService> logger) {
            _botService = botService;
            _crawlerService = crawlerService;
            _crawlerConfig = crawlerConfig;
            _imageService = imageService;
            _logger = logger;
        }

        async Task<bool> IUpdateService.AddBatchStickersToSetAsync (List<Telegram.Bot.Types.File> files, int userId, string name, string pngSticker, int emoji) {
            try {
                for (int i = 0; i < files.Count; i++) {
                    var emojiString = char.ConvertFromUtf32 (emoji + i);
                    await _botService.Client.AddStickerToSetAsync (
                        userId: userId,
                        name: name,
                        pngSticker: files[i].FileId,
                        emojis: emojiString
                    );
                }
                return true;
            } catch (Exception ex) {
                _logger.LogError ($"AddBatchStickersToSetAsync():[Error:{ex}]");
            }
            return false;
        }

        public async Task EchoAsync (Update update) {
            var inputMsg = update.Message;
            var message = string.Empty;

            var tempFolder = $"\\{Guid.NewGuid().ToString("N")}_Img";
            Directory.CreateDirectory (Directory.GetCurrentDirectory () + tempFolder);
            var currentPath = Directory.GetCurrentDirectory () + tempFolder;
            try {
                if (update.Type == UpdateType.Message) {
                    // if (false) {
                    string urlPattern = $"{_crawlerConfig.Value.LineStickerHost}*";
                    Match checkedUrl = Regex.Match (inputMsg.Text, urlPattern);

                    if (checkedUrl.Success) {
                        await _botService.Client.SendTextMessageAsync (
                            chatId: inputMsg.Chat.Id,
                            text: "Line 連結正確,開始轉換貼圖"
                        );

                        var imageUrls = await _crawlerService.GetLineStickerUrlsAsync (inputMsg.Text);
                        List<Telegram.Bot.Types.File> files = new List<Telegram.Bot.Types.File> ();

                        foreach (var url in imageUrls) {
                            var imgName = $"\\{Guid.NewGuid ().ToString("N")}.png";
                            var path = currentPath + imgName;
                            using (var wc = new WebClient ()) {
                                wc.DownloadFile (new Uri (url), path);
                            }
                            var file = await _imageService.UploadResizeImagesToTG (_botService.Client, path, (int) inputMsg.Chat.Id);
                            files.Add (file);
                        }

                        string tempName = $"bot{Guid.NewGuid ().ToString ("N")}_by_martinwangBot";

                        int firstEmojiUnicode = 0x1F601;
                        var emojiString = char.ConvertFromUtf32 (firstEmojiUnicode);
                        var stickerTitle = await _crawlerService.GetTextBySelector (
                            inputMsg.Text, "div.mdCMN38Item0lHead>.mdCMN38Item01Ttl");

                        await _botService.Client.CreateNewStickerSetAsync (
                            userId: (int) inputMsg.Chat.Id,
                            name : tempName,
                            title : stickerTitle,
                            pngSticker : files[0].FileId,
                            emojis : emojiString
                        );

                        for (int i = 0; i < files.Count; i++) {
                            emojiString = char.ConvertFromUtf32 (firstEmojiUnicode + i);
                            await _botService.Client.AddStickerToSetAsync (
                                userId: (int) inputMsg.Chat.Id,
                                name : tempName,
                                pngSticker : files[i].FileId,
                                emojis : emojiString
                            );
                        }

                        var stickerSet = await _botService.Client.GetStickerSetAsync (
                            name: tempName
                        );

                        var sticker = stickerSet.Stickers[0];

                        await _botService.Client.SendStickerAsync (
                            chatId: new ChatId (inputMsg.Chat.Id),
                            sticker: new Telegram.Bot.Types.InputFiles.InputOnlineFile (sticker.FileId)
                        );

                    } else {
                        await _botService.Client.SendTextMessageAsync (
                            chatId: inputMsg.Chat.Id,
                            text: $" Please, Send me a line sticker link "
                        );
                    }
                    Directory.Delete (currentPath, true);
                }
            } catch (Exception ex) {
                _logger.LogError ($"{ex}");
                Directory.Delete (currentPath, true);
                await _botService.Client.SendTextMessageAsync (
                    chatId: inputMsg.Chat.Id,
                    text: $"please send correct Line sticker link"
                );

            }
        }
    }
}