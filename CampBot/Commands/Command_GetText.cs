using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CampBot.Commands
{
    internal class Command_GetText : ICommand
    {
        public string GetName { get => "def"; set { } }
        public async void Execute(TelegramBotClient _bot, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await _bot.EditMessageTextAsync(
                callbackQuery.Message.Chat.Id,
                callbackQuery.Message.MessageId,
                "Выберете ваши дальнейшие действия",
                replyMarkup: GetKeyboard());
        }

        public InlineKeyboardMarkup GetKeyboard()
        {
            return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
            {
                new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData($"Информация о людях", $"GCH:null"),
                },
                new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData($"Выбор места", $"CPS:null")
                }

            });
        }
    }
}
