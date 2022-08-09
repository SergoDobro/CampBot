using CampBot.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using User = CampBot.Classes.User;

namespace CampBot.Commands
{
    internal class GetChildren : ICommand
    {
        Storage _storage;
        public void SetStorage(Storage storage)
        {
            _storage = storage;
        }
        public string GetName { get => "GCH"; set { } }
        int a = 0;
        public async void Execute(TelegramBotClient _bot, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            List<string> GetUsers = (await _storage.GetUser(callbackQuery.From.Id)).Children;
            var inlines = new List<List<InlineKeyboardButton>>() { };
            string text = "Информация о людях:\n\n";
            if (GetUsers != null)
            {
                for (int i = 0; i < GetUsers.Count; i++)
                {
                    var user = await _storage.GetUserByUserName(GetUsers[i]);
                    string name =(user.Name != null && user.Name != "" ? user.Name : user.UserName);
                    if (user.Location!=null && user.Location!="")
                        text += $"{name} сейчас в {(await _storage.GetPlace(user.Location)).WhereAreYou}\n";
                    else
                        text += $"{name} неизвестно где, не отмечался\n";
                }

            }
            //if (GetUsers != null)
            //{
            //    for (int i = 0; i < GetUsers.Count; i++)
            //    {
            //        inlines.Add(new List<InlineKeyboardButton>()
            //    {
            //        InlineKeyboardButton.WithCallbackData($"{(await _storage.GetUser(GetUsers[i])).Name}", $"GCH:{GetUsers[i]}"),
            //    });
            //    }

            //}
            inlines.Add(
                new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData($"<---", $"def:Вернуться"),
                });
            //if (callbackQuery.Data.Split(':').Length > 1)
            //{
            //    long idd = 0;
            //    if (long.TryParse(callbackQuery.Data.Split(':')[1], out idd))
            //    {
            //        User user = await _storage.GetUser(idd);
            //        text = "Этот человек сейчас в "+user.Location;
            //    }
            //}
            try
            {
                await _bot.EditMessageTextAsync(
                    callbackQuery.Message.Chat.Id,
                    callbackQuery.Message.MessageId,
                    text,
                    replyMarkup: new InlineKeyboardMarkup(inlines));
            }
            catch
            {

            }
        }

        public InlineKeyboardMarkup GetKeyboard()
        {
            return new InlineKeyboardMarkup(new List<List<InlineKeyboardButton>>()
            {
                new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData($"Обновить", $"upd"),
                },
                new List<InlineKeyboardButton>()
                {
                    InlineKeyboardButton.WithCallbackData($"<---", $"def:Вернуться"),
                }

            });
        }
    }
}
