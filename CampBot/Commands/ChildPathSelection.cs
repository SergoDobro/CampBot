using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using CampBot.Classes;

namespace CampBot.Commands
{
    internal class ChildPathSelection : ICommand
    {
        Storage _storage;
        public void SetStorage(Storage storage)
        {
            _storage = storage;
        }
        public string GetName { get => "CPS"; set { } }
        int a = 0;
        public async void Execute(TelegramBotClient _bot, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            //storage set
            if (callbackQuery.Data.Split(":")[1] != "null")
            {
                await _storage.SetChildLocation(callbackQuery.Message.Chat.Id, callbackQuery.Data.Split(":")[1]);
            }
            try
            {
                var user = await _storage.GetUser(callbackQuery.Message.Chat.Id);
                if (user.Location == null)
                {
                    await _bot.EditMessageTextAsync(
                        callbackQuery.Message.Chat.Id,
                        callbackQuery.Message.MessageId,
                        "Мы не знаем где вы...",
                        replyMarkup: GetKeyboard());
                }
                else
                {
                    if (user.Location == "null")
                    {
                        await _bot.EditMessageTextAsync(
                            callbackQuery.Message.Chat.Id,
                            callbackQuery.Message.MessageId,
                            "Мы не знаем где вы...",
                            replyMarkup: GetKeyboard());
                    }
                    else
                    {
                        await _bot.EditMessageTextAsync(
                            callbackQuery.Message.Chat.Id,
                            callbackQuery.Message.MessageId,
                            "Вы сейчас в " + (await _storage.GetPlace(user.Location)).WhereAreYou,
                            replyMarkup: GetKeyboard());
                    }
                }
            }
            catch
            {

            }
        }

        public InlineKeyboardMarkup GetKeyboard()
        {
            List<List<InlineKeyboardButton>> keyboardButtons = new List<List<InlineKeyboardButton>>();
            var allPlaces = _storage.GetAllPlaces();
            foreach (var item in allPlaces)
            {
                keyboardButtons.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData($"{item.PlaceName}", $"{GetName}:{item.Tag}"),
                });
            }
            keyboardButtons.Add(new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData($"<---", $"def:Вернуться"),
                });
            return new InlineKeyboardMarkup(keyboardButtons);
        }
    }
}
