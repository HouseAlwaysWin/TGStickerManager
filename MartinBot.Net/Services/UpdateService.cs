using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MartinBot.Net.Config;
using MartinBot.Net.Services.interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MartinBot.Net.Services {
    public class UpdateService : IUpdateService {

        private readonly ITelegramBotClient _botService;
        private readonly ILogger<UpdateService> _logger;
        private readonly GlobalConfig _config;

        public UpdateService (
            ITelegramBotClient botService,
            GlobalConfig config,
            ILogger<UpdateService> logger) {
            _botService = botService;
            _logger = logger;
            _config = config;
        }

        async Task<bool> IUpdateService.AddBatchStickersToSetAsync (List<Telegram.Bot.Types.File> files, int userId, string name, string pngSticker, int emoji) {
            try {
                for (int i = 0; i < files.Count; i++) {
                    var emojiString = char.ConvertFromUtf32 (emoji + i);
                    await _botService.AddStickerToSetAsync (
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
            if (update.Type == UpdateType.Message) {

                var lineId = Regex.Match (inputMsg.Text, @"\d+").Value;
                var downloadUrl = string.Format (_config.LineStickerConfig.NormalStickerUrl, lineId);
                List<Telegram.Bot.Types.File> files = new List<Telegram.Bot.Types.File> ();
                using (var hc = new HttpClient ()) {
                    Stream imgsZip = null;
                    try {
                        imgsZip = await hc.GetStreamAsync (downloadUrl);
                    } catch (Exception ex) {
                        _logger.Log (LogLevel.Error, $"{ex}");
                        return;
                    }
                    using (ZipArchive zipFile = new ZipArchive (imgsZip)) {
                        foreach (var entry in zipFile.Entries) {
                            if (Regex.Match (entry.Name, @"^\d+@2x.png").Success) {
                                using (Image image = Image.Load (entry.Open ()))
                                using (MemoryStream ms = new MemoryStream ()) {
                                    image.Mutate (m => m.Resize (512, 512));
                                    image.SaveAsPng (ms);
                                    ms.Seek (0, SeekOrigin.Begin);
                                    try {
                                        var file = await _botService.UploadStickerFileAsync (
                                            userId: (int) inputMsg.Chat.Id,
                                            pngSticker : new Telegram.Bot.Types.InputFiles.InputFileStream (ms)
                                        );
                                        files.Add (file);
                                    } catch (Exception ex) {
                                        _logger.Log (LogLevel.Error, $"{ex}");
                                        return;
                                    }
                                }
                            }
                        };
                    }
                }

                string tempName = $"bot{Guid.NewGuid ().ToString ("N")}_by_{_config.BotConfig.BotName}";

                int firstEmojiUnicode = 0x1F601;
                var emojiString = char.ConvertFromUtf32 (firstEmojiUnicode);
                var stickerTitle = "TransferBy@MartinBot";

                await _botService.CreateNewStickerSetAsync (
                    userId: (int) inputMsg.Chat.Id,
                    name : tempName,
                    title : stickerTitle,
                    pngSticker : files[0].FileId,
                    emojis : emojiString
                );

                for (int i = 0; i < files.Count; i++) {
                    emojiString = char.ConvertFromUtf32 (firstEmojiUnicode + i);
                    await _botService.AddStickerToSetAsync (
                        userId: (int) inputMsg.Chat.Id,
                        name : tempName,
                        pngSticker : files[i].FileId,
                        emojis : emojiString
                    );
                }

                var stickerSet = await _botService.GetStickerSetAsync (
                    name: tempName
                );

                var sticker = stickerSet.Stickers[0];

                await _botService.SendStickerAsync (
                    chatId: new ChatId (inputMsg.Chat.Id),
                    sticker: new Telegram.Bot.Types.InputFiles.InputOnlineFile (sticker.FileId)
                );

            } else {
                await _botService.SendTextMessageAsync (
                    chatId: inputMsg.Chat.Id,
                    text: $" Please, Send me a line sticker link "
                );
            }

        }
    }
}