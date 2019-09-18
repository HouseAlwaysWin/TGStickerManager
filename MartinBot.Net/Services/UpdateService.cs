using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MartinBot.Net.Services.interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MartinBot.Net.Services {
    public class UpdateService : IUpdateService {

        private readonly IBotService _botService;
        private readonly ICrawlerService _crawlerService;
        private readonly IImageService _imageService;
        private readonly ILogger<UpdateService> _logger;

        public UpdateService (
            IBotService botService,
            ICrawlerService crawlerService,
            IImageService imageService,
            ILogger<UpdateService> logger) {
            _botService = botService;
            _crawlerService = crawlerService;
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
            try {
                var inputMsg = update.Message;

                if (update.Type == UpdateType.Message) {
                    var message = await _botService.Client.SendTextMessageAsync (
                        chatId: inputMsg.Chat.Id,
                        text: $"Choose tool you want to use",
                        parseMode : ParseMode.Markdown,
                        replyMarkup : new InlineKeyboardMarkup (new [] {
                            InlineKeyboardButton.WithCallbackData ("LineStickerTransfer", "Line"),
                        })
                    );
                    // if (inputMsg.Text == "/start") {

                    // Message message = await _botService.Client.SendTextMessageAsync (
                    //     chatId: inputMsg.Chat.Id,
                    //     text: "Choose category you want:",
                    //     parseMode : ParseMode.Markdown,
                    //     replyMarkup : new InlineKeyboardMarkup (new [] {
                    //         InlineKeyboardButton.WithCallbackData ("Music", "this is music"),
                    //             InlineKeyboardButton.WithCallbackData ("Theme", "theme"),
                    //             InlineKeyboardButton.WithCallbackData ("Sport", "sport"),
                    //             InlineKeyboardButton.WithCallbackData ("Program", "program"),
                    //             InlineKeyboardButton.WithCallbackData ("Game", "game")
                    //     })
                    // );
                    // }
                } else if (update.Type == UpdateType.CallbackQuery) {
                    switch (update.CallbackQuery.Data) {
                        case "Line":
                            await _botService.Client.SendTextMessageAsync (update.CallbackQuery.Message.Chat.Id, "Start Transfer");
                            break;
                    }
                }
            } catch (Exception ex) {
                _logger.LogError ($"{ex}");
            }

        }

    }
}