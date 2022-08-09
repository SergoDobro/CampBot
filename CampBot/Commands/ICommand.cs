using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace CampBot.Commands
{
    internal interface ICommand
    {
        public string GetName { get; set; }
        public InlineKeyboardMarkup GetKeyboard();
        public void Execute(TelegramBotClient _bot, CallbackQuery callbackQuery, CancellationToken cancellationToken);

        //{
        //    return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
        //    {
        //        new List<InlineKeyboardButton>()
        //        {
        //            InlineKeyboardButton.WithCallbackData($"{likes.Count} 👍", $"r:{id}:like"),
        //            InlineKeyboardButton.WithCallbackData($"{dislikes.Count} 👎", $"r:{id}:dislike")
        //        }

        //    });
        //}
    }
}
