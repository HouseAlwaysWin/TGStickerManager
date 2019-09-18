using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MartinBot.Net.Services {
    public class UpdateService : IUpdateService {

        private readonly IBotService _botService;
        private readonly ILogger<UpdateService> _logger;

        public UpdateService (IBotService botService, ILogger<UpdateService> logger) {
            _botService = botService;
            _logger = logger;
        }

        public async Task EchoAsync (Update update) {

            try {
                var inputMsg = update.Message;

                if (update.Type == UpdateType.Message) {

                    if (inputMsg.Text == "/start") {

                        Message message = await _botService.Client.SendTextMessageAsync (
                            chatId: inputMsg.Chat.Id,
                            text: "Choose category you want:",
                            parseMode : ParseMode.Markdown,
                            replyMarkup : new InlineKeyboardMarkup (new [] {
                                InlineKeyboardButton.WithCallbackData ("Music", "this is music"),
                                    InlineKeyboardButton.WithCallbackData ("Theme", "theme"),
                                    InlineKeyboardButton.WithCallbackData ("Sport", "sport"),
                                    InlineKeyboardButton.WithCallbackData ("Program", "program"),
                                    InlineKeyboardButton.WithCallbackData ("Game", "game")
                            })
                        );

                    }
                } else if (update.Type == UpdateType.CallbackQuery) {
                    await _botService.Client.SendTextMessageAsync (update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Data);
                }
            } catch (Exception ex) {

            }

        }
    }
}