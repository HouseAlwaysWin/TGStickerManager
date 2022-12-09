using Telegram.Bot.Types;

namespace TGStickerAPI.Models
{
    public class BotCommandInfo : BotCommand
    {
        public Action<Message> Action { get; set; }

        public BotCommandInfo(string command, string description, Action<Message> action)
        {
            Command = command;
            Description = description;
            Action = action;
        }
    }
}
